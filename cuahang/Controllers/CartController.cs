using Microsoft.AspNetCore.Mvc;
using cuahang.Models;
using Microsoft.EntityFrameworkCore;

namespace cuahang.Controllers
{
    public class HoaDonController : Controller
    {
        private readonly ApplicationDbContext _db;

        public HoaDonController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            var orders = _db.HoaDon
                            .Include(h => h.User)
                            .OrderByDescending(h => h.NgayDat)
                            .ToList();
            return View(orders);
        }

        [HttpPost]
        public async Task<IActionResult> SaveOrder([FromBody] HoaDon model)
        {
            try
            {
                if (model == null)
                {
                    return Json(new { success = false, message = "Dữ liệu không hợp lệ." });
                }

                if (string.IsNullOrEmpty(model.Id))
                {
                    model.Id = Guid.NewGuid().ToString();
                }

                model.NgayDat = DateTime.Now;
                model.TrangThai = "Pending";

                if (model.ChiTietHoaDons != null)
                {
                    foreach (var detail in model.ChiTietHoaDons)
                    {
                        detail.HoaDonId = model.Id;
                    }
                }

                _db.HoaDon.Add(model);
                await _db.SaveChangesAsync();

                return Json(new { success = true, orderId = model.Id });
            }
            catch (Exception ex)
            {
                string error = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return Json(new { success = false, message = error });
            }
        }
    }
}