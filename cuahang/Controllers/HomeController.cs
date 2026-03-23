using cuahang.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace cuahang.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _;
        private readonly ApplicationDbContext _db;
        public HomeController(ApplicationDbContext db) { _db = db; }

        

        

        public IActionResult Index()
        {
            if (!_db.SanPham.Any()) // khi ko có sản phẩm
            {
                _db.SanPham.Add(new SanPham
                {
                    TenSP = "iPhone 15 Pro",
                    Gia = 25000000,
                    HinhAnh = "0",
                    MoTa = "Điện thoại cao cấp",
                    SoLuongTon = 1,
                    ImageUrl="iphone.webp"
                });
                _db.SaveChanges();
            }
            var dsSanPham = _db.SanPham.ToList();
            return View(dsSanPham);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
