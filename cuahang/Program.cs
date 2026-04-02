using cuahang;
using cuahang.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Cấu hình Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Cấu hình Session (Chỉ khai báo 1 LẦN DUY NHẤT)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Giỏ hàng/Phiên đăng nhập tồn tại trong 30p
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".Cuahang.Session";
});

// 3. Đăng ký các dịch vụ MVC và HttpContext
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// 4. Cấu hình Pipeline xử lý Request (THỨ TỰ RẤT QUAN TRỌNG)
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Phục vụ file css, js, hình ảnh

app.UseRouting(); // Định tuyến

// 5. Kích hoạt Session (Phải nằm SAU UseRouting và TRƯỚC UseAuthorization)
app.UseSession();

app.UseAuthorization(); // Phân quyền

// 6. Cấu hình Route mặc định
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// 7. Chạy ứng dụng (Dòng này luôn nằm CUỐI CÙNG)
app.Run();