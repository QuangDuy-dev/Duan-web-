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

        public IActionResult Index(string keyword, string type, string brand)
        {
            var dsSanPham = _db.SanPham.Include(s => s.ChiTietHoaDons).AsQueryable();
            if (!string.IsNullOrEmpty(keyword))
            {
                dsSanPham = dsSanPham.Where(p => p.TenSP.Contains(keyword));
            }
            if (!string.IsNullOrEmpty(type))
            {
                dsSanPham = dsSanPham.Where(p => p.LoaiSp.StartsWith(type));
            }

            if (!string.IsNullOrEmpty(brand))
            {
                dsSanPham = dsSanPham.Where(p => p.LoaiSp.EndsWith(" " + brand));
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

            var brandGroups = new Dictionary<string, List<dynamic>>
            {
                { "1", new List<dynamic> { 
                    new { Code = "ip", Name = "Apple" }, new { Code = "ss", Name = "Samsung" },
                    new { Code = "xm", Name = "Xiaomi" }, new { Code = "nb", Name = "Nubia" },
                    new { Code = "sn", Name = "Sony" }, new { Code = "vv", Name = "Vivo" },
                    new { Code = "px", Name = "Pixel" }
                }},
                { "2", new List<dynamic> { 
                    new { Code = "ss", Name = "Samsung" }, new { Code = "ak", Name = "Anker" },
                    new { Code = "tc", Name = "Transcend" }, new { Code = "sd", Name = "Sandisk" },
                    new { Code = "dji", Name = "DJI" }, new { Code = "ct", Name = "Cuktech" },
                    new { Code = "atk", Name = "ATK" }, new { Code = "mad", Name = "MAD" },
                    new { Code = "rz", Name = "Razer" }, new { Code = "cs", Name = "Corsair" },
                    new { Code = "ip", Name = "Apple" }, new { Code = "lg", Name = "LG" },
                    new { Code = "asus", Name = "Asus" }, new { Code = "lenovo", Name = "Lenovo" },
                    new { Code = "msi", Name = "MSI" }, new { Code = "sn", Name = "Sony" }
                }}
            };

            ViewBag.DisplayBrands = !string.IsNullOrEmpty(type) && brandGroups.ContainsKey(type) ? brandGroups[type] : new List<dynamic>();
            ViewBag.CurrentType = type;
            ViewBag.CurrentBrand = brand;
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
