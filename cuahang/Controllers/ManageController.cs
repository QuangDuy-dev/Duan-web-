using cuahang;
using cuahang.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http; // Thêm để dùng Session
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

public class ManageController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _environment;

    public ManageController(ApplicationDbContext db, IWebHostEnvironment environment)
    {
        _db = db;
        _environment = environment;
    }

    // Hàm hỗ trợ kiểm tra quyền Admin (giữ logic từ đoạn 2)
    private bool IsAdmin()
    {
        return HttpContext.Session.GetString("UserLevel") == "2";
    }

    [HttpGet]
    public JsonResult GetRevenueData(string type)
    {
        // Thêm kiểm tra quyền từ đoạn 2
        if (!IsAdmin()) return Json(new { error = "Unauthorized" });

        var query = _db.HoaDon.AsQueryable();
        object labels = null;
        object values = null;

        if (type == "day")
        {
            var startDate = DateTime.Now.Date.AddDays(-6);
            var data = query.Where(h => h.NgayDat >= startDate)
                            .GroupBy(h => h.NgayDat.Date)
                            .Select(g => new { Date = g.Key, Total = g.Sum(h => h.TongTien) })
                            .OrderBy(x => x.Date)
                            .ToList();

            labels = data.Select(x => x.Date.ToString("dd/MM")).ToArray();
            values = data.Select(x => x.Total).ToArray();
        }
        else if (type == "month")
        {
            var data = query.Where(h => h.NgayDat.Year == DateTime.Now.Year)
                            .GroupBy(h => h.NgayDat.Month)
                            .Select(g => new { Month = g.Key, Total = g.Sum(h => h.TongTien) })
                            .OrderBy(x => x.Month)
                            .ToList();

            labels = data.Select(x => "Tháng " + x.Month).ToArray();
            values = data.Select(x => x.Total).ToArray();
        }
        else // Year
        {
            var data = query.GroupBy(h => h.NgayDat.Year)
                            .Select(g => new { Year = g.Key, Total = g.Sum(h => h.TongTien) })
                            .OrderBy(x => x.Year)
                            .ToList();

            labels = data.Select(x => x.Year.ToString()).ToArray();
            values = data.Select(x => x.Total).ToArray();
        }

        return Json(new { labels, values });
    }

    public IActionResult LichSuDonHangAdmin()
    {
        // Cập nhật logic điều hướng từ đoạn 2
        if (!IsAdmin()) return RedirectToAction("Index", "Home");

        var tatCaDonHang = _db.HoaDon
            .Include(h => h.User)
            .OrderByDescending(h => h.NgayDat)
            .ToList();

        ViewBag.TongDoanhThu = tatCaDonHang.Sum(x => x.TongTien);
        ViewBag.TongDonHang = tatCaDonHang.Count;

        return View(tatCaDonHang);
    }

    [HttpPost]
    public IActionResult UpdateStatus(int id, string status)
    {
        if (!IsAdmin()) return Json(new { success = false });

        var hoadon = _db.HoaDon.Find(id);
        if (hoadon != null)
        {
            hoadon.TrangThai = status;
            _db.SaveChanges();
            return Json(new { success = true });
        }
        return Json(new { success = false, message = "Không tìm thấy đơn hàng" });
    }

    public IActionResult Manage()
    {
        if (!IsAdmin()) return RedirectToAction("Index", "Home");
        var data = _db.SanPham.ToList();
        return View(data);
    }

    [HttpPost]
    public IActionResult Create(SanPham sp, IFormFile? imageFile)
    {
        if (!IsAdmin()) return RedirectToAction("Index", "Home");

        if (sp.Gia < 0 || sp.SoLuongTon < 0)
        {
            TempData["Error"] = "Giá và số lượng không được là số âm.";
            return RedirectToAction("Manage");
        }

        var uploadResult = SaveProductImage(imageFile, sp.ImageUrl);
        if (!uploadResult.Success)
        {
            TempData["Error"] = uploadResult.Message;
            return RedirectToAction("Manage");
        }

        if (string.IsNullOrEmpty(sp.HinhAnh)) sp.HinhAnh = "0";
        if (string.IsNullOrEmpty(sp.DanhGia)) sp.DanhGia = "0";
        sp.ImageUrl = uploadResult.FileName ?? sp.ImageUrl;

        _db.SanPham.Add(sp);
        _db.SaveChanges();
        TempData["Success"] = $"Đã thêm sản phẩm {sp.TenSP} thành công!";
        return RedirectToAction("Manage");
    }


    [HttpPost]
    public IActionResult Edit(SanPham sp, IFormFile? imageFile)
    {
        if (!IsAdmin()) return Json(new { success = false, message = "Không có quyền truy cập" });

        if (sp.Gia < 0 || sp.SoLuongTon < 0)
            return Json(new { success = false, message = "Giá và số lượng không được là số âm" });

        var existing = _db.SanPham.Find(sp.Id);
        if (existing == null)
            return Json(new { success = false, message = "Không tìm thấy sản phẩm" });

        var uploadResult = SaveProductImage(imageFile, existing.ImageUrl);
        if (!uploadResult.Success)
            return Json(new { success = false, message = uploadResult.Message });

        var previousImageUrl = existing.ImageUrl;

        existing.TenSP = sp.TenSP;
        existing.Gia = sp.Gia;
        existing.SoLuongTon = sp.SoLuongTon;
        existing.ImageUrl = uploadResult.FileName ?? existing.ImageUrl;
        existing.LoaiSp = sp.LoaiSp;
        existing.MoTa = sp.MoTa;

        _db.SaveChanges();
        if (!string.Equals(previousImageUrl, existing.ImageUrl, StringComparison.OrdinalIgnoreCase))
        {
            DeleteProductImageIfUnused(previousImageUrl);
        }

        return Json(new { success = true, imageUrl = existing.ImageUrl });
    }

    private (bool Success, string? FileName, string? Message) SaveProductImage(IFormFile? imageFile, string? currentImageUrl)
    {
        if (imageFile == null || imageFile.Length == 0)
            return (true, currentImageUrl, null);

        var allowedExtensions = new[] { ".png", ".jpg", ".jpeg", ".webp" };
        var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(extension))
            return (false, null, "Chỉ hỗ trợ file ảnh PNG, JPG, JPEG hoặc WEBP.");

        var imageFolder = Path.Combine(_environment.WebRootPath, "image");
        Directory.CreateDirectory(imageFolder);

        var fileName = GenerateNextWebpFileName(imageFolder);
        var destinationPath = Path.Combine(imageFolder, fileName);
        var tempFolder = Path.Combine(Path.GetTempPath(), "cuahang-image-upload");
        Directory.CreateDirectory(tempFolder);
        var tempPath = Path.Combine(tempFolder, $"{Guid.NewGuid():N}.tmp");

        try
        {
            using (var inputStream = imageFile.OpenReadStream())
            using (var image = Image.Load(inputStream))
            {
                image.SaveAsWebp(tempPath, new WebpEncoder
                {
                    Quality = 85
                });
            }

            if (System.IO.File.Exists(destinationPath))
            {
                System.IO.File.Delete(destinationPath);
            }

            System.IO.File.Move(tempPath, destinationPath);
        }
        finally
        {
            if (System.IO.File.Exists(tempPath))
            {
                System.IO.File.Delete(tempPath);
            }
        }

        return (true, fileName, null);
    }

    private string GenerateNextWebpFileName(string imageFolder)
    {
        var pattern = new Regex(@"^IMG_(\d{3})\.WEBP$", RegexOptions.IgnoreCase);
        var nextNumber = Directory
            .EnumerateFiles(imageFolder, "IMG_*.WEBP")
            .Select(Path.GetFileName)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => pattern.Match(name!))
            .Where(match => match.Success)
            .Select(match => int.Parse(match.Groups[1].Value))
            .DefaultIfEmpty(0)
            .Max() + 1;

        return $"IMG_{nextNumber:D3}.WEBP";
    }

    public IActionResult Delete(int id)
    {
        if (!IsAdmin()) return RedirectToAction("Index", "Home");
        var sp = _db.SanPham.Find(id);
        if (sp != null)
        {
            string ten = sp.TenSP;
            string? imageUrl = sp.ImageUrl;
            _db.SanPham.Remove(sp);
            _db.SaveChanges();
            DeleteProductImageIfUnused(imageUrl);
            TempData["Success"] = $"Đã xóa sản phẩm {ten}!";
        }
        return RedirectToAction("Manage");
    }

    private void DeleteProductImageIfUnused(string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
            return;

        var normalizedFileName = Path.GetFileName(imageUrl.Trim());
        if (string.IsNullOrWhiteSpace(normalizedFileName))
            return;

        var isStillUsed = _db.SanPham.Any(x => x.ImageUrl == normalizedFileName);
        if (isStillUsed)
            return;

        var imagePath = Path.Combine(_environment.WebRootPath, "image", normalizedFileName);
        if (System.IO.File.Exists(imagePath))
        {
            System.IO.File.Delete(imagePath);
        }
    }

    public IActionResult Details(int id)
    {
        if (!IsAdmin()) return RedirectToAction("Index", "Home");
        var sp = _db.SanPham.Find(id);
        return View(sp);
    }

    public IActionResult News()
    {
        if (!IsAdmin()) return RedirectToAction("Index", "Home");
        var news = _db.BaiBao.OrderByDescending(x => x.NgayDang).ToList();
        return View(news);
    }

    [HttpPost]
    public IActionResult CreateNews(BaiBao bb)
    {
        if (!IsAdmin()) return RedirectToAction("Index", "Home");
        bb.NgayDang = DateTime.Now; // Cập nhật ngày đăng tự động
        _db.BaiBao.Add(bb);
        _db.SaveChanges();
        TempData["Success"] = "Đăng bài viết mới thành công!";
        return RedirectToAction("News");
    }

    public IActionResult DeleteNews(int id)
    {
        if (!IsAdmin()) return RedirectToAction("Index", "Home");
        var bb = _db.BaiBao.Find(id);
        if (bb != null)
        {
            _db.BaiBao.Remove(bb);
            _db.SaveChanges();
            TempData["Success"] = "Đã xóa bài viết!";
        }
        return RedirectToAction("News");
    }

    // --- CÁC PHẦN MỚI TỪ ĐOẠN 2 ĐƯỢC THÊM VÀO CUỐI ĐỂ TRÁNH SUNG ĐỘT ---

    public IActionResult ManageKhuyenMai()
    {
        if (!IsAdmin()) return RedirectToAction("Index", "Home");
        var list = _db.KhuyenMai.OrderByDescending(x => x.Id).ToList();
        return View(list);
    }

    [HttpPost]
    public IActionResult CreateKhuyenMai(KhuyenMai km)
    {
        if (!IsAdmin()) return RedirectToAction("Index", "Home");

        if (km.NgayHetHang < DateTime.Now.Date)
        {
            TempData["Error"] = "Ngày hết hạn không thể ở quá khứ!";
            return RedirectToAction("ManageKhuyenMai");
        }

        _db.KhuyenMai.Add(km);
        _db.SaveChanges();
        TempData["Success"] = $"Chúc mừng bạn đã tạo thành công voucher {km.KMName}!";
        return RedirectToAction("ManageKhuyenMai");
    }

    public IActionResult DeleteKhuyenMai(int id)
    {
        if (!IsAdmin()) return RedirectToAction("Index", "Home");
        var km = _db.KhuyenMai.Find(id);
        if (km != null)
        {
            string code = km.KMName;
            _db.KhuyenMai.Remove(km);
            _db.SaveChanges();
            TempData["Success"] = $"Đã xóa mã khuyến mãi {code}.";
        }
        return RedirectToAction("ManageKhuyenMai");
    }

    public IActionResult Logout()
    {
        // Xóa Session nếu cần hoặc chỉ đơn giản là redirect
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home");
    }

}
