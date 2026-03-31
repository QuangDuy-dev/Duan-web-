using BCrypt.Net;
using cuahang.Models;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using System.Text.RegularExpressions;

namespace cuahang.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        // Khai báo các biến dùng chung để quản lý quy tắc mật khẩu
        private const string PasswordPattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$";
        private const string PasswordErrorMessage = "Mật khẩu phải có ít nhất 8 ký tự, bao gồm chữ hoa, chữ thường và số.";

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // Xóa sạch các biến toàn cục
            return RedirectToAction("index", "home");
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            // Sử dụng LINQ để tìm người dùng trong DB Online
            var user = _context.nguoidung.FirstOrDefault(u => u.Name == username);

            if (user != null && BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                // LƯU BIẾN TOÀN CỤC (Session)
                HttpContext.Session.SetInt32("UserId", user.Id);
                HttpContext.Session.SetString("UserName", user.Name);
                HttpContext.Session.SetString("UserEmail", user.Email);
                // KIEM TRA QUYỀN (lvID) ĐỂ PHÂN QUYỀN TRONG ỨNG DỤNG
                HttpContext.Session.SetString("UserLevel", user.lvID ?? "1");
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
        public IActionResult Register(string username, string password, string email, string confirmPassword)
        {
            try
            {
                // Kiểm tra định dạng mật khẩu bằng biến dùng chung
                if (!Regex.IsMatch(password, PasswordPattern))
                {
                    TempData["Error"] = PasswordErrorMessage;
                    return View();
                }

                if (password != confirmPassword)
                {
                    TempData["Error"] = "Mật khẩu xác nhận không khớp. Vui lòng thử lại!";
                    return View();
                }
                // 1. Kiểm tra xem tên đăng nhập đã tồn tại chưa (Tùy chọn nhưng nên làm)
                var existingUser = _context.nguoidung.FirstOrDefault(u => u.Name == username);
                if (existingUser != null)
                {
                    TempData["Error"] = "Tên đăng nhập này đã có người sử dụng!";
                    return View();
                }
                // 2. Kiểm tra trùng Email 
                var existingEmail = _context.nguoidung.FirstOrDefault(u => u.Email == email);
                if (existingEmail != null)
                {
                    TempData["Error"] = "Email này đã được đăng ký bởi một tài khoản khác!";
                    return View();
                }
                // 3. Tạo đối tượng mới để ghi vào DB
                var newUser = new Models.User
                {
                    Name = username,
                    Password = BCrypt.Net.BCrypt.HashPassword(password),
                    Email = email,
                    OTP = "0",
                    lvID = "1"
                };

                // 4. Lệnh LINQ để thêm và lưu
                _context.nguoidung.Add(newUser);
                _context.SaveChanges();

                // 5. Báo thành công và chuyển hướng về trang Login
                TempData["Success"] = "Đăng ký thành công! Bạn có thể đăng nhập ngay.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi hệ thống: " + ex.Message;
                return View();
            }
        }

        // Trang quên mật khẩu 
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SendOTP(string email)
        {
            var user = _context.nguoidung.FirstOrDefault(u => u.Email == email);
            if (user != null)
            {
                // 1. Tạo mã OTP
                string otp = new Random().Next(100000, 999999).ToString();
                user.OTP = otp;
                _context.SaveChanges();

                try
                {
                    // 2. Cấu hình nội dung Email
                    var message = new MimeMessage();
                    message.From.Add(new MailboxAddress("Hệ thống Web", "giakietngo2812@gmail.com"));
                    message.To.Add(new MailboxAddress("", email));
                    message.Subject = "Mã xác thực OTP Reset Password";
                    message.Body = new TextPart("plain")
                    {
                        Text = $"Mã xác thực của bạn là: {otp}. Vui lòng không cung cấp mã này cho bất kỳ ai."
                    };

                    // 3. Gửi Email qua SMTP của Gmail
                    using (var client = new SmtpClient())
                    {
                        client.Connect("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);

                        client.Authenticate("giakietngo2812@gmail.com", "xzyp lecl upsj nawu");

                        client.Send(message);
                        client.Disconnect(true);
                    }
                    return Ok();
                }
                catch (Exception ex)
                {

                    return StatusCode(500, ex.Message);
                }
            }
            return BadRequest();
        }

        [HttpGet]
        public IActionResult ChangePassWord(string email)
        {
            if (string.IsNullOrEmpty(email)) return RedirectToAction("ForgotPassword");

            ViewBag.Email = email;
            return View();
        }

        [HttpPost]
        public IActionResult VerifyOTP(string email, string otp)
        {
            var user = _context.nguoidung.FirstOrDefault(u => u.Email == email && u.OTP == otp);

            if (user != null)
            {
                TempData["Success"] = "Xác thực OTP thành công!";
                ViewBag.Email = email;
                return RedirectToAction("ChangePassWord", new { email = email });
            }

            TempData["Error"] = "Mã OTP không chính xác. Vui lòng kiểm tra lại!";
            ViewBag.Email = email;
            return View("ForgotPassword");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ConfirmChangePassword(string email, string newPassword, string confirmPassword)
        {
            // Kiểm tra định dạng mật khẩu mới bằng biến dùng chung
            if (!Regex.IsMatch(newPassword, PasswordPattern))
            {
                TempData["Error"] = PasswordErrorMessage;
                ViewBag.Email = email;
                return View("ChangePassword");
            }

            // 1. Kiểm tra mật khẩu có khớp nhau không
            if (newPassword != confirmPassword)
            {
                TempData["Error"] = "Mật khẩu xác nhận không khớp!";
                ViewBag.Email = email;
                return View("ChangePassword");
            }

            // 2. Tìm người dùng trong database theo Email
            var user = _context.nguoidung.FirstOrDefault(u => u.Email == email);

            if (user != null)
            {
                // 3. Băm mật khẩu mới bằng BCrypt trước khi lưu
                user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);

                // 4. Reset mã OTP về 0 để không dùng lại được nữa
                user.OTP = "0";

                _context.SaveChanges();

                // 5. Thông báo thành công và chuyển về trang Login
                TempData["Success"] = "Đổi mật khẩu thành công! Vui lòng đăng nhập lại.";
                return RedirectToAction("Login");
            }

            TempData["Error"] = "Đã có lỗi xảy ra, vui lòng thử lại!";
            return View("ForgotPassword");
        }
        
      

        public IActionResult LichSuDonHangUser() //khachhang
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            // Lấy danh sách hóa đơn của User này, kèm theo chi tiết sản phẩm
            var lichSu = _context.HoaDon
                .Include(h => h.ChiTietHoaDons)
                .ThenInclude(ct => ct.SanPham)
                .Where(h => h.UserId == userId)
                .OrderByDescending(h => h.NgayDat)
                .ToList();

            return View(lichSu);
        }

        public IActionResult ChiTietDonHang(int id)
        {
            // Lấy thông tin hóa đơn và các chi tiết sản phẩm đi kèm
            var hoaDon = _context.HoaDon
                .Include(h => h.ChiTietHoaDons) // Load bảng chi tiết
                .ThenInclude(ct => ct.SanPham)  // Load thông tin Sản phẩm để lấy Tên, Hình ảnh
                .FirstOrDefault(h => h.Id == id);

            if (hoaDon == null)
            {
                return NotFound();
            }

            return View(hoaDon);
        }
    }
}