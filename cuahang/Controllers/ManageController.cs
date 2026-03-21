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
}