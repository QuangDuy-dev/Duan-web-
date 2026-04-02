using cuahang.Helpers;
using cuahang.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;

namespace cuahang.Controllers
{
    public class CartController(ApplicationDbContext context) : Controller
    {
        private readonly ApplicationDbContext _context = context;

        // 1. Trang danh sách giỏ hàng (QUAN TRỌNG: Thiếu cái này sẽ bị lỗi 404)
        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetJson<List<CartItem>>("GioHang") ?? new List<CartItem>();
            ViewBag.TongTien = cart.Sum(s => s.ThanhTien);
            return View(cart);
        }

        // 2. Lấy số lượng item trong giỏ (Dùng cho Header)
        public IActionResult GetCartCount()
        {
            var cart = HttpContext.Session.GetJson<List<CartItem>>("GioHang") ?? [];
            return Json(new { count = cart.Sum(x => x.SoLuong) });
        }

        // 3. Thêm sản phẩm vào giỏ
        [HttpPost]
        public IActionResult AddToCart(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập để mua hàng!" });
            }

            var cart = HttpContext.Session.GetJson<List<CartItem>>("GioHang") ?? [];
            var item = cart.FirstOrDefault(p => p.SanPhamId == id);

            // Tìm sản phẩm trong kho để kiểm tra trước khi thêm
            var product = _context.SanPham.Find(id);
            if (product == null) return Json(new { success = false, message = "Sản phẩm không tồn tại!" });

            if (item != null)
            {
                // Logic bổ sung: Kiểm tra nếu thêm 1 nữa có vượt quá kho không
                if (item.SoLuong + 1 > product.SoLuongTon)
                {
                    return Json(new { success = false, message = "Số lượng trong kho không đủ!" });
                }
                item.SoLuong++;
            }
            else
            {
                // Kiểm tra nếu sản phẩm còn hàng hay không
                if (product.SoLuongTon <= 0)
                {
                    return Json(new { success = false, message = "Sản phẩm đã hết hàng!" });
                }

                cart.Add(new CartItem
                {
                    SanPhamId = product.Id,
                    TenSP = product.TenSP,
                    Gia = product.Gia,
                    SoLuong = 1,
                    ImageUrl = product.ImageUrl
                });
            }

            HttpContext.Session.SetJson("GioHang", cart);

            var pName = product.TenSP;
            return Json(new { success = true, productName = pName, count = cart.Sum(x => x.SoLuong) });
        }

        // 4. Cập nhật số lượng trong trang giỏ hàng
        [HttpPost]
        public IActionResult UpdateQuantity(int id, int quantity)
        {
            var cart = HttpContext.Session.GetJson<List<CartItem>>("GioHang") ?? [];
            var item = cart.FirstOrDefault(p => p.SanPhamId == id);

            if (item != null)
            {
                // Kiểm tra tồn kho khi người dùng thay đổi số lượng bằng tay
                var product = _context.SanPham.Find(id);
                if (product != null && quantity > product.SoLuongTon)
                {
                    quantity = product.SoLuongTon; // Chỉ cho phép đặt tối đa bằng số tồn kho
                }

                item.SoLuong = quantity > 0 ? quantity : 1;
                HttpContext.Session.SetJson("GioHang", cart);
            }

            return Json(new
            {
                success = true,
                grandTotal = cart.Sum(x => x.ThanhTien).ToString("N0"),
                cartCount = cart.Sum(x => x.SoLuong)
            });
        }

        // 5. Xóa khỏi giỏ hàng
        [HttpPost]
        public IActionResult RemoveFromCart(int id)
        {
            var cart = HttpContext.Session.GetJson<List<CartItem>>("GioHang") ?? [];
            var item = cart.FirstOrDefault(p => p.SanPhamId == id);
            if (item != null)
            {
                cart.Remove(item);
                HttpContext.Session.SetJson("GioHang", cart);
            }
            return Json(new { success = true, count = cart.Sum(x => x.SoLuong) });
        }

        // 6. API Kiểm tra mã giảm giá (Dùng AJAX ở trang Checkout)
        [HttpPost]
        public IActionResult CheckVoucher(string code)
        {
            var cart = HttpContext.Session.GetJson<List<CartItem>>("GioHang") ?? [];
            decimal totalAmount = cart.Sum(x => x.ThanhTien);

            var km = _context.KhuyenMai.FirstOrDefault(v => v.KMName == code);

            if (km == null) return Json(new { success = false, message = "Mã giảm giá không tồn tại!" });
            if (km.NgayHetHang < DateTime.Now) return Json(new { success = false, message = "Mã đã hết hạn!" });
            if (km.KMSoluong <= 0) return Json(new { success = false, message = "Mã đã hết lượt sử dụng!" });
            if (totalAmount < km.KMDieuKien)
                return Json(new { success = false, message = $"Đơn hàng tối thiểu {km.KMDieuKien:N0}đ mới dùng được mã này!" });

            decimal discount = totalAmount * km.HeSoGiam / 100;
            if (discount > km.GiamToiDa) discount = km.GiamToiDa;

            return Json(new
            {
                success = true,
                discountAmount = discount,
                message = "Áp dụng mã thành công!",
                newTotal = totalAmount - discount
            });
        }

        // 7. Trang xác nhận đơn hàng
        [HttpGet]
        public IActionResult Checkout()
        {
            var cart = HttpContext.Session.GetJson<List<CartItem>>("GioHang") ?? [];
            if (!cart.Any()) return RedirectToAction("Index");

            ViewBag.TongTienHang = cart.Sum(x => x.ThanhTien);
            return View(cart);
        }

        // 8. Xử lý đặt hàng và trừ kho khuyến mãi
        [HttpPost]
        public IActionResult ConfirmOrder(string DiaChi, string SoDienThoai, string KMCode, decimal GiamGiaValue)
        {
            var cart = HttpContext.Session.GetJson<List<CartItem>>("GioHang") ?? [];
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null || !cart.Any()) return RedirectToAction("Login", "Account");

            // Dùng Transaction để đảm bảo tính toàn vẹn: 
            // Nếu một bước lỗi (như trừ kho lỗi), toàn bộ hóa đơn sẽ không được tạo.
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    decimal tongTienHang = cart.Sum(x => x.ThanhTien);

                    // Cập nhật số lượng mã khuyến mãi nếu có dùng
                    if (!string.IsNullOrEmpty(KMCode))
                    {
                        var km = _context.KhuyenMai.FirstOrDefault(v => v.KMName == KMCode);
                        if (km != null && km.KMSoluong > 0)
                        {
                            km.KMSoluong--;
                            _context.Update(km);
                        }
                    }

                    // Lưu Hóa Đơn
                    var hoaDon = new HoaDon
                    {
                        NgayDat = DateTime.Now,
                        UserId = userId.Value,
                        DiaChi = DiaChi,
                        SoDienThoai = SoDienThoai,
                        GiamGia = GiamGiaValue,
                        TongTien = tongTienHang - GiamGiaValue,
                        TrangThai = "Chờ xử lý"
                    };

                    _context.HoaDon.Add(hoaDon);
                    _context.SaveChanges();

                    // Lưu Chi Tiết Hóa Đơn VÀ CẬP NHẬT TRỪ KHO SẢN PHẨM
                    foreach (var item in cart)
                    {
                        // 1. Tìm sản phẩm trong database
                        var product = _context.SanPham.Find(item.SanPhamId);
                        if (product == null || product.SoLuongTon < item.SoLuong)
                        {
                            // Nếu hàng trong kho không đủ tại thời điểm nhấn thanh toán
                            throw new Exception($"Sản phẩm {item.TenSP} đã hết hàng hoặc không đủ số lượng!");
                        }

                        // 2. Trừ số lượng tồn kho
                        product.SoLuongTon -= item.SoLuong;
                        _context.SanPham.Update(product);

                        // 3. Thêm chi tiết hóa đơn
                        _context.ChiTietHoaDon.Add(new ChiTietHoaDon
                        {
                            HoaDonId = hoaDon.Id,
                            SanPhamId = item.SanPhamId,
                            SoLuong = item.SoLuong,
                            DonGia = item.Gia
                        });
                    }

                    _context.SaveChanges();
                    transaction.Commit(); // Xác nhận hoàn tất tất cả thay đổi

                    HttpContext.Session.Remove("GioHang");
                    return RedirectToAction("OrderSuccess");
                }
                catch (Exception ex)
                {
                    transaction.Rollback(); // Hủy bỏ nếu có bất kỳ lỗi nào xảy ra
                    TempData["Error"] = ex.Message;
                    return RedirectToAction("Checkout");
                }
            }
        }

        public IActionResult OrderSuccess()
        {
            ViewBag.UserName = HttpContext.Session.GetString("UserName") ?? "Quý khách";
            return View();
        }
    }
}