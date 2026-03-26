using System.Diagnostics;
using cuahang.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace cuahang.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _;
        private readonly ApplicationDbContext _db;
        public HomeController(ApplicationDbContext db) { _db = db; }

        public IActionResult Index(string keyword)
        {
            var dsSanPham = _db.SanPham.AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                dsSanPham = dsSanPham.Where(p => p.TenSP.Contains(keyword));
            }

            if (!_db.SanPham.Any())
            {
                _db.SanPham.AddRange(
                    new SanPham
                    {
                        TenSP = "iPhone 15 Pro",
                        Gia = 25000000,
                        ImageUrl = "iphone.webp",
                        MoTa = "Điện thoại cao cấp",
                        SoLuongTon = 1
                    },
                    new SanPham
                    {
                        TenSP = "Samsung Galaxy S26 Ultra",
                        Gia = 30000000,
                        ImageUrl = "s26_ultra.webp",
                        MoTa = "Điện thoại cao cấp",
                        SoLuongTon = 1
                    }
                );
                _db.SaveChanges();
            }

            return View(dsSanPham.ToList());
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
