using cuahang.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace cuahang.Controllers
{
    public class NewsController : Controller
    {
        private readonly ApplicationDbContext _db;
        public NewsController(ApplicationDbContext db) { _db = db; }

        public IActionResult Index()
        {
            var news = _db.BaiBao.OrderByDescending(x => x.NgayDang).ToList();
            return View(news);
        }

        public IActionResult Details(int id)
        {
            var baiBao = _db.BaiBao.Find(id);
            if (baiBao == null)
            {
                return NotFound(); 
            }
            return View(baiBao); 
        }
    }
}