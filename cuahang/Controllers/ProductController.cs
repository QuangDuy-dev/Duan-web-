using cuahang.Models;
using Microsoft.AspNetCore.Mvc;

namespace cuahang.Controllers
{
  
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _db;
        public ProductController(ApplicationDbContext db) { _db = db; }
       
    }
}
