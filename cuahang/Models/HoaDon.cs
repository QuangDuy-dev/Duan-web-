using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace cuahang.Models
{
    public class HoaDon
    {
        public int Id { get; set; }

        public DateTime NgayDat { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TongTien { get; set; }

        public string TrangThai { get; set; } = "Pending";

        // --- CÁC TRƯỜNG MỚI CẦN BỔ SUNG ---

        [Display(Name = "Địa chỉ giao hàng")]
        public string? DiaChi { get; set; }

        [Display(Name = "Số điện thoại")]
        public string? SoDienThoai { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Số tiền giảm giá")]
        public decimal GiamGia { get; set; } = 0;

        // ----------------------------------

        [ForeignKey("User")]
        public int UserId { get; set; }
        public virtual User User { get; set; }

        // Một hóa đơn có nhiều chi tiết hóa đơn
        public virtual ICollection<ChiTietHoaDon> ChiTietHoaDons { get; set; } = new List<ChiTietHoaDon>();
    }
}