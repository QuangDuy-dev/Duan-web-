using cuahang;
using cuahang.Models;
using Microsoft.AspNetCore.Mvc;

public class ManageController : Controller
{
    private readonly ApplicationDbContext _db;

    public ManageController(ApplicationDbContext db)
    {
        _db = db;
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
        sp.DanhGia = "0";

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