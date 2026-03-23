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
                if (quantity > 0)
                {
                    item.SoLuong = quantity;
                }
                else
                {
                    item.SoLuong = 1;
                }

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

        // 3. Thêm vào giỏ
        [HttpPost]
        public IActionResult AddToCart(int id)
        {
           
            var cart = HttpContext.Session.GetJson<List<CartItem>>("GioHang") ?? [];
            var item = cart.FirstOrDefault(p => p.SanPhamId == id);

            if (item != null)
            {
                item.SoLuong++;
            }
            else
            {
                var product = _context.SanPham.Find(id);

                if (product == null)
                {
                    return Json(new { success = false, message = "Sản phẩm không tồn tại!" });
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

            return Json(new
            {
                success = true,
                productName = cart.FirstOrDefault(p => p.SanPhamId == id)?.TenSP,
                count = cart.Sum(x => x.SoLuong)
            });
        }

        // 4. Trang danh sách giỏ hàng
        public IActionResult Index()
        {
            
            var cart = HttpContext.Session.GetJson<List<CartItem>>("GioHang") ?? [];
            ViewBag.TongTien = cart.Sum(s => s.ThanhTien);
            return View(cart);
        }

        // 5. Xóa khỏi giỏ
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

        // 6. Action xử lý tính toán Voucher và hiển thị trang xác nhận thanh toán
        [HttpGet]
        public IActionResult Checkout()
        {
            
            var cart = HttpContext.Session.GetJson<List<CartItem>>("GioHang") ?? [];

            if (!cart.Any())
            {
                return RedirectToAction("Index");
            }

            decimal tongTienHang = cart.Sum(x => x.ThanhTien);
            decimal voucherGiam = 0;
            int phanTramGiam = 0;

            if (tongTienHang > 100000000)
            {
                phanTramGiam = 35;
            }
            else if (tongTienHang > 70000000)
            {
                phanTramGiam = 25;
            }
            else if (tongTienHang > 50000000)
            {
                phanTramGiam = 15;
            }

            voucherGiam = tongTienHang * phanTramGiam / 100;

            ViewBag.TongTienHang = tongTienHang;
            ViewBag.VoucherGiam = voucherGiam;
            ViewBag.PhanTramGiam = phanTramGiam;
            ViewBag.TongThanhToan = tongTienHang - voucherGiam;

            return View(cart);
        }

        [HttpPost]
        public IActionResult ConfirmOrder(string DiaChi, string SoDienThoai)
        {
            if (string.IsNullOrEmpty(DiaChi) || string.IsNullOrEmpty(SoDienThoai))
            {
                return RedirectToAction("Checkout");
            }

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