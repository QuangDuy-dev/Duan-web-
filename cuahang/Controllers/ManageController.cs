using cuahang;
using cuahang.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class ManageController : Controller
{
    private readonly ApplicationDbContext _db;

    public ManageController(ApplicationDbContext db)
    {
        _db = db;
    }
    [HttpGet]
    public JsonResult GetRevenueData(string type)
    {
        var query = _db.HoaDon.AsQueryable();
        object labels = null;
        object values = null;

        if (type == "day")
        {
            // Lấy doanh thu 7 ngày gần nhất
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
            // Lấy doanh thu các tháng trong năm hiện tại
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
        // Kiểm tra quyền Admin (ví dụ qua lvID)
        var lvID = HttpContext.Session.GetString("UserLevel");
        if (lvID != "2") return Content("Bạn không có quyền truy cập");

        // 1. Lấy toàn bộ lịch sử đơn hàng của tất cả mọi người
        var tatCaDonHang = _db.HoaDon
            .Include(h => h.User) // Để biết ai mua
            .OrderByDescending(h => h.NgayDat)
            .ToList();

        // 2. Thống kê doanh thu
        ViewBag.TongDoanhThu = tatCaDonHang.Sum(x => x.TongTien);
        ViewBag.TongDonHang = tatCaDonHang.Count;

        return View(tatCaDonHang);
    }

    [HttpPost]
    public IActionResult UpdateStatus(int id, string status)
    {
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
        var data = _db.SanPham.ToList();
        return View(data);
    }

    [HttpPost]
    public IActionResult Create(SanPham sp)
    {
        sp.HinhAnh = "0";

        _db.SanPham.Add(sp);
        _db.SaveChanges();
        return RedirectToAction("Manage");
    }

    public IActionResult Delete(int id)
    {
        var sp = _db.SanPham.Find(id);
        if (sp != null)
        {
            _db.SanPham.Remove(sp);
            _db.SaveChanges();
        }
        return RedirectToAction("Manage");
    }



    public IActionResult Details(int id)
    {
        var sp = _db.SanPham.Find(id);
        return View(sp);
    }

    //Trang danh sách bài báo trong Mangage
    public IActionResult News()
    {
        var news = _db.BaiBao.OrderByDescending(x => x.NgayDang).ToList();
        return View(news);
    }

    // Xử lý thêm bài báo
    [HttpPost]
    public IActionResult CreateNews(BaiBao bb)
    {
        _db.BaiBao.Add(bb);
        _db.SaveChanges();
        return RedirectToAction("News");
    }

    // Xóa bài báo
    public IActionResult DeleteNews(int id)
    {
        var bb = _db.BaiBao.Find(id);
        if (bb != null)
        {
            _db.BaiBao.Remove(bb);
            _db.SaveChanges();
        }
        return RedirectToAction("News");
    }
}