using System.ComponentModel.DataAnnotations.Schema;

namespace cuahang.Models
{
    public class HoaDon
    {
        public string Id { get; set; }
        public DateTime NgayDat { get; set; } = DateTime.Now;
        public decimal TongTien { get; set; }
        public string TrangThai { get; set; } = "Pending";

        [ForeignKey("User")]
        // Khóa ngoại liên kết tới User (nguoidung)
        public int UserId { get; set; }
        public virtual User User { get; set; }

        // Một hóa đơn có nhiều chi tiết hóa đơn
        public virtual ICollection<ChiTietHoaDon> ChiTietHoaDons { get; set; }
    }
}
