using cuahang.Models;
using Microsoft.EntityFrameworkCore;

namespace cuahang

{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> nguoidung { get; set; }
    }
}
