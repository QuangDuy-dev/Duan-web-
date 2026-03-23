using cuahang.Helpers;
using cuahang.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace cuahang.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Lấy số lượng tổng để cập nhật Badge trên Header
        [HttpGet]
        public IActionResult GetCartCount()
        {
            var cart = HttpContext.Session.GetJson<List<CartItem>>("GioHang") ?? new List<CartItem>();
            return Json(new { count = cart.Sum(x => x.SoLuong) });
        }

        // 2. HOÀN THIỆN: Cập nhật số lượng và trả về dữ liệu tính toán mới
        [HttpPost]
        public IActionResult UpdateQuantity(int id, int quantity)
        {
            var cart = HttpContext.Session.GetJson<List<CartItem>>("GioHang") ?? new List<CartItem>();
            var item = cart.FirstOrDefault(p => p.SanPhamId == id);

            if (item != null)
            {
                if (quantity > 0)
                {
                    item.SoLuong = quantity;
                }
                else
                {
                    // Nếu số lượng về 0 hoặc nhỏ hơn, có thể xóa khỏi giỏ hoặc giữ tối thiểu là 1
                    item.SoLuong = 1;
                }

                // Lưu lại danh sách đã cập nhật vào Session
                HttpContext.Session.SetJson("GioHang", cart);
            }

            // Tính toán lại các con số để trả về cho giao diện AJAX cập nhật ngay lập tức
            return Json(new
            {
                success = true,
                totalItemPrice = item?.ThanhTien.ToString("N0"), // Tổng tiền của riêng sản phẩm này
                grandTotal = cart.Sum(x => x.ThanhTien).ToString("N0"), // Tổng cộng cả giỏ hàng
                cartCount = cart.Sum(x => x.SoLuong) // Tổng số lượng sản phẩm để cập nhật Badge
            });
        }

        // 3. Thêm vào giỏ
        [HttpPost]
        public IActionResult AddToCart(int id)
        {
            var cart = HttpContext.Session.GetJson<List<CartItem>>("GioHang") ?? new List<CartItem>();
            var item = cart.FirstOrDefault(p => p.SanPhamId == id);

            if (item != null)
            {
                item.SoLuong++;
            }
            else
            {
                // Lưu ý: Đổi thành _context.SanPhams nếu bạn đã sửa tên bảng trong DbContext
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
            var cart = HttpContext.Session.GetJson<List<CartItem>>("GioHang") ?? new List<CartItem>();
            ViewBag.TongTien = cart.Sum(s => s.ThanhTien);
            return View(cart);
        }

        // 5. Xóa khỏi giỏ
        [HttpPost]
        public IActionResult RemoveFromCart(int id)
        {
            var cart = HttpContext.Session.GetJson<List<CartItem>>("GioHang") ?? new List<CartItem>();
            var item = cart.FirstOrDefault(p => p.SanPhamId == id);

            if (item != null)
            {
                cart.Remove(item);
                HttpContext.Session.SetJson("GioHang", cart);
            }

            return Json(new { success = true, count = cart.Sum(x => x.SoLuong) });
        }
    }
}