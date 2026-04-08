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

        public async Task<IActionResult> Index(string keyword, string type, string brand, decimal? minPrice, decimal? maxPrice)
        {
            
            var dsSanPham = _db.SanPham.Include(s => s.ChiTietHoaDons).AsQueryable();

            if (minPrice.HasValue)
            {
                dsSanPham = dsSanPham.Where(p => p.Gia >= minPrice.Value);
            }
            if (maxPrice.HasValue)
            {
                dsSanPham = dsSanPham.Where(p => p.Gia <= maxPrice.Value);
            }

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

            // Get unique LoaiSp
            var uniqueLoaiStrings = await _db.SanPham
                .Select(s => s.LoaiSp)
                .Distinct()
                .ToListAsync();

            var brandGroups = uniqueLoaiStrings
                .Where(l => !string.IsNullOrWhiteSpace(l) && l.Contains(" "))
                .Select(l => 
                {
                    var parts = l.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    return new { CatId = parts[0], Code = parts[1] };
                })
                .GroupBy(x => x.CatId)
                .ToDictionary(
                    g => g.Key, 
                    g => g.Select(x => (dynamic)new 
                    { 
                        Code = x.Code, 
                        Name = GetBrandDisplayName(x.Code) 
                    }).ToList()
                );
            decimal maxDbPrice = await _db.SanPham.AnyAsync() ? await _db.SanPham.MaxAsync(p => p.Gia) : 50000000;
            ViewBag.DisplayBrands = !string.IsNullOrEmpty(type) && brandGroups.ContainsKey(type) 
                                    ? brandGroups[type] 
                                    : new List<dynamic>();
            ViewBag.CurrentType = type;
            ViewBag.CurrentBrand = brand;
            ViewBag.MinPrice = minPrice ?? 0;
            ViewBag.MaxPrice = maxPrice ?? maxDbPrice;
            ViewBag.MaxDbPrice = maxDbPrice;
            return View(await dsSanPham.ToListAsync());
            
        }
        private string GetBrandDisplayName(string code)
        {
            return code.ToLower() switch
            {
                "ss" => "Samsung", "msi" => "MSI", "lenovo" => "Lenovo",
                "xm" => "Xiaomi", "lg"=>"LG", "asus" => "Asus",
                "sn" => "Sony", "rz" => "Razer", "cs"=> "Corsair",
                "ak" => "Anker", "mad" => "Madlions", "ip" => "Apple",
                "nb" => "Nubia", "ct" =>"Cuktech", "atk" => "Atk",
                "vv" => "Vivo", "sd" => "SanDisk", "dji" => "DJI", 
                "px" => "Pixel", "tc" => "Transcend",
                _=> char.ToUpper(code[0]) + code.Substring(1)
            };
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

        public async Task<IActionResult> Details(int? id)
{
    if (id == null) return NotFound();

    // Include ChiTietHoaDons to calculate ratings
    var sanPham = await _db.SanPham
        .Include(s => s.ChiTietHoaDons)
        .FirstOrDefaultAsync(m => m.Id == id);

    if (sanPham == null) return NotFound();

    // Brand Logic
    string brandName = "";
    string brandCode = "";
    if (!string.IsNullOrEmpty(sanPham.LoaiSp) && sanPham.LoaiSp.Contains(" "))
    {
        var parts = sanPham.LoaiSp.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        brandCode = parts[1];
        brandName = GetBrandDisplayName(brandCode);
    }

    ViewBag.BrandName = brandName;
    ViewBag.BrandCode = brandCode;

    return View(sanPham);
}
}
}
