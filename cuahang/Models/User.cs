using System.ComponentModel.DataAnnotations.Schema;
namespace cuahang.Models
{
    [Table("nguoidung")]
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string OTP { get; set; }
        public string lvID {  get; set; } 

        public virtual ICollection<HoaDon> HoaDons { get; set; }

    }
}
