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

        public IActionResult GetCartCount()
        {
            var cart = HttpContext.Session.GetJson<List<CartItem>>("GioHang") ?? [];
            return Json(new { count = cart.Sum(x => x.SoLuong) });
        }

        [HttpPost]
        public IActionResult UpdateQuantity(int id, int quantity)
        {
            var cart = HttpContext.Session.GetJson<List<CartItem>>("GioHang") ?? [];
            var item = cart.FirstOrDefault(p => p.SanPhamId == id);

            if (item != null)
            {
                item.SoLuong = quantity > 0 ? quantity : 1;
                HttpContext.Session.SetJson("GioHang", cart);
            }

            return Json(new
            {
                success = true,
                totalItemPrice = item?.ThanhTien.ToString("N0"),
                grandTotal = cart.Sum(x => x.ThanhTien).ToString("N0"),
                cartCount = cart.Sum(x => x.SoLuong)
            });
        }

        [HttpPost]
        public IActionResult AddToCart(int id)
        {
            // Kiểm tra đăng nhập qua Session UserId
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập để thêm vào giỏ hàng!" });
            }

            var cart = HttpContext.Session.GetJson<List<CartItem>>("GioHang") ?? [];
            var item = cart.FirstOrDefault(p => p.SanPhamId == id);

            if (item != null)
            {
                item.SoLuong++;
            }
            else
            {
                var product = _context.SanPham.Find(id);
                if (product == null) return Json(new { success = false, message = "Sản phẩm không tồn tại!" });

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
            return Json(new { success = true, productName = cart.FirstOrDefault(p => p.SanPhamId == id)?.TenSP, count = cart.Sum(x => x.SoLuong) });
        }

        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetJson<List<CartItem>>("GioHang") ?? [];
            ViewBag.TongTien = cart.Sum(s => s.ThanhTien);
            return View(cart);
        }

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

        [HttpGet]
        public IActionResult Checkout()
        {
            var cart = HttpContext.Session.GetJson<List<CartItem>>("GioHang") ?? [];
            if (!cart.Any()) return RedirectToAction("Index");

            decimal tongTienHang = cart.Sum(x => x.ThanhTien);
            int phanTramGiam = tongTienHang > 100000000 ? 35 : (tongTienHang > 70000000 ? 25 : (tongTienHang > 50000000 ? 15 : 0));
            decimal voucherGiam = tongTienHang * phanTramGiam / 100;

            ViewBag.TongTienHang = tongTienHang;
            ViewBag.VoucherGiam = voucherGiam;
            ViewBag.PhanTramGiam = phanTramGiam;
            ViewBag.TongThanhToan = tongTienHang - voucherGiam;

            return View(cart);
        }

        [HttpPost]
        public IActionResult ConfirmOrder(string DiaChi, string SoDienThoai)
        {
            var cart = HttpContext.Session.GetJson<List<CartItem>>("GioHang") ?? [];
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null || !cart.Any()) return RedirectToAction("Login", "Account");

            // --- TÍNH TOÁN LẠI VOUCHER ĐỂ LƯU CHÍNH XÁC ---
            decimal tongTienHang = cart.Sum(x => x.ThanhTien);
            int phanTramGiam = tongTienHang > 100000000 ? 35 : (tongTienHang > 70000000 ? 25 : (tongTienHang > 50000000 ? 15 : 0));
            decimal voucherGiam = tongTienHang * phanTramGiam / 100;
            decimal tongPhaiTra = tongTienHang - voucherGiam;

            // 1. Tạo hóa đơn mới và GÁN CÁC THÔNG TIN CÒN THIẾU
            var hoaDon = new HoaDon
            {
                NgayDat = DateTime.Now,
                UserId = userId.Value,
                DiaChi = DiaChi,             // QUAN TRỌNG: Gán địa chỉ khách nhập
                SoDienThoai = SoDienThoai,   // QUAN TRỌNG: Gán số điện thoại khách nhập
                GiamGia = voucherGiam,       // QUAN TRỌNG: Lưu số tiền đã giảm
                TongTien = tongPhaiTra,      // QUAN TRỌNG: Lưu số tiền cuối cùng sau khi trừ voucher
                TrangThai = "Chờ xử lý"
            };

            _context.HoaDon.Add(hoaDon);
            _context.SaveChanges();

            // 2. Lưu chi tiết hóa đơn
            foreach (var item in cart)
            {
                var ct = new ChiTietHoaDon
                {
                    HoaDonId = hoaDon.Id,
                    SanPhamId = item.SanPhamId,
                    SoLuong = item.SoLuong,
                    DonGia = item.Gia
                };
                _context.ChiTietHoaDon.Add(ct);
            }

            _context.SaveChanges();
            HttpContext.Session.Remove("GioHang");

            return RedirectToAction("OrderSuccess");
        }

        public IActionResult OrderSuccess()
        {
            ViewBag.UserName = HttpContext.Session.GetString("UserName") ?? "Quý khách";
            return View();
        }
    }
}