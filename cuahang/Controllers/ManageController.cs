using cuahang;
using cuahang.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http; // Thêm để dùng Session
using System;
using System.Linq;

public class ManageController : Controller
{
    private readonly ApplicationDbContext _db;

    public ManageController(ApplicationDbContext db)
    {
        _db = db;
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
    public IActionResult Create(SanPham sp)
    {
        if (!IsAdmin()) return RedirectToAction("Index", "Home");

        if (string.IsNullOrEmpty(sp.HinhAnh)) sp.HinhAnh = "0";
        if (string.IsNullOrEmpty(sp.DanhGia)) sp.DanhGia = "0";

        _db.SanPham.Add(sp);
        _db.SaveChanges();
        TempData["Success"] = $"Đã thêm sản phẩm {sp.TenSP} thành công!";
        return RedirectToAction("Manage");
    }

    [HttpPost]
    public IActionResult Edit([FromBody] SanPham sp)
    {
        var existing = _db.SanPham.Find(sp.Id);
        if (existing == null)
            return Json(new { success = false, message = "Không tìm thấy sản phẩm" });

        existing.TenSP = sp.TenSP;
        existing.Gia = sp.Gia;
        existing.SoLuongTon = sp.SoLuongTon;
        existing.ImageUrl = sp.ImageUrl;
        existing.LoaiSp = sp.LoaiSp;
        existing.MoTa = sp.MoTa;

        _db.SaveChanges();
        return Json(new { success = true });
    }

    public IActionResult Delete(int id)
    {
        if (!IsAdmin()) return RedirectToAction("Index", "Home");
        var sp = _db.SanPham.Find(id);
        if (sp != null)
        {
            string ten = sp.TenSP;
            _db.SanPham.Remove(sp);
            _db.SaveChanges();
            TempData["Success"] = $"Đã xóa sản phẩm {ten}!";
        }
        return RedirectToAction("Manage");
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