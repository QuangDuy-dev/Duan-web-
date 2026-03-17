using Microsoft.AspNetCore.Mvc;

namespace cuahang.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Login()
        {
            return View();
        }

        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            // Sử dụng LINQ để tìm người dùng trong DB Online
            var user = _context.nguoidung
                               .FirstOrDefault(u => u.Name == username && u.Password == password);

            if (user != null)
            {
                // Nếu tìm thấy, chuyển hướng sang trang chủ
                return RedirectToAction("Index", "Home");
            }

            TempData["Error"] = "Tài khoản hoặc mật khẩu không chính xác!";
            
            return View();
        }

        // Trang Đăng ký
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(string username, string password,string email)
        {
            try
            {
                // 1. Kiểm tra xem tên đăng nhập đã tồn tại chưa (Tùy chọn nhưng nên làm)
                var existingUser = _context.nguoidung.FirstOrDefault(u => u.Name == username);
                if (existingUser != null)
                {
                    TempData["Error"] = "Tên đăng nhập này đã có người sử dụng!";
                    return View();
                }

                // 2. Tạo đối tượng mới để ghi vào DB
                var newUser = new Models.User
                {
                    Name = username,
                    Password = password,
                    Email = email, 
                    OTP = "0",                // Gán mặc định
                    lvID = "1"
                };

                // 3. Lệnh LINQ để thêm và lưu
                _context.nguoidung.Add(newUser);
                _context.SaveChanges();

                // 4. Báo thành công và chuyển hướng về trang Login
                TempData["Success"] = "Đăng ký thành công! Bạn có thể đăng nhập ngay.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi hệ thống: " + ex.Message;
                return View();
            }
        }
    }
}
// day la test 2 