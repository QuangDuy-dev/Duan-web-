using cuahang.Helpers;
using cuahang.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace cuahang.Controllers
{
    public class CartController(ApplicationDbContext context) : Controller
    {
        private readonly ApplicationDbContext _context = context;

        // 1. Trang danh sách giỏ hàng
        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetJson<List<CartItem>>("GioHang") ?? new List<CartItem>();
            ViewBag.TongTien = cart.Sum(s => s.ThanhTien);
            return View(cart);
        }

        // 2. Lấy số lượng item trong giỏ
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

            var product = _context.SanPham.Find(id);
            if (product == null) return Json(new { success = false, message = "Sản phẩm không tồn tại!" });

            if (item != null)
            {
                if (item.SoLuong + 1 > product.SoLuongTon)
                {
                    return Json(new { success = false, message = "Số lượng trong kho không đủ!" });
                }
                item.SoLuong++;
            }
            else
            {
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
            return Json(new { success = true, productName = product.TenSP, count = cart.Sum(x => x.SoLuong) });
        }

        // 4. CẬP NHẬT SỐ LƯỢNG (ĐÃ SỬA LỖI UNDEFINED)
        [HttpPost]
        public IActionResult UpdateQuantity(int id, int quantity)
        {
            var cart = HttpContext.Session.GetJson<List<CartItem>>("GioHang") ?? [];
            var item = cart.FirstOrDefault(p => p.SanPhamId == id);
            string totalItemStr = "0";

            if (item != null)
            {
                var product = _context.SanPham.Find(id);
                if (product != null && quantity > product.SoLuongTon)
                {
                    quantity = product.SoLuongTon;
                }

                item.SoLuong = quantity > 0 ? quantity : 1;
                totalItemStr = item.ThanhTien.ToString("N0"); // Lấy thành tiền của SP này

                HttpContext.Session.SetJson("GioHang", cart);
            }

            return Json(new
            {
                success = true,
                totalItemPrice = totalItemStr, // TRẢ VỀ BIẾN NÀY ĐỂ HẾT LỖI UNDEFINED
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

        // 6. Kiểm tra mã giảm giá
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

        // 8. Xử lý đặt hàng
        [HttpPost]
        public IActionResult ConfirmOrder(string DiaChi, string SoDienThoai, string KMCode, decimal GiamGiaValue)
        {
            var cart = HttpContext.Session.GetJson<List<CartItem>>("GioHang") ?? [];
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null || !cart.Any()) return RedirectToAction("Login", "Account");

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    if (!string.IsNullOrEmpty(KMCode))
                    {
                        var km = _context.KhuyenMai.FirstOrDefault(v => v.KMName == KMCode);
                        if (km != null && km.KMSoluong > 0)
                        {
                            km.KMSoluong--;
                            _context.Update(km);
                        }
                    }

                    var hoaDon = new HoaDon
                    {
                        NgayDat = DateTime.Now,
                        UserId = userId.Value,
                        DiaChi = DiaChi,
                        SoDienThoai = SoDienThoai,
                        GiamGia = GiamGiaValue,
                        TongTien = cart.Sum(x => x.ThanhTien) - GiamGiaValue,
                        TrangThai = "Chờ xử lý"
                    };

                    _context.HoaDon.Add(hoaDon);
                    _context.SaveChanges();

                    foreach (var item in cart)
                    {
                        var product = _context.SanPham.Find(item.SanPhamId);
                        if (product == null || product.SoLuongTon < item.SoLuong)
                        {
                            throw new Exception($"Sản phẩm {item.TenSP} đã hết hàng!");
                        }

                        product.SoLuongTon -= item.SoLuong;
                        _context.SanPham.Update(product);

                        _context.ChiTietHoaDon.Add(new ChiTietHoaDon
                        {
                            HoaDonId = hoaDon.Id,
                            SanPhamId = item.SanPhamId,
                            SoLuong = item.SoLuong,
                            DonGia = item.Gia
                        });
                    }

                    _context.SaveChanges();
                    transaction.Commit();

                    HttpContext.Session.Remove("GioHang");
                    return RedirectToAction("OrderSuccess");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
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