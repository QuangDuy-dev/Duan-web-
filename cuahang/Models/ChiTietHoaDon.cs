using System.ComponentModel.DataAnnotations.Schema;

namespace cuahang.Models
{
    public class ChiTietHoaDon
    {
        public int Id { get; set; }
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }

        [ForeignKey("HoaDon")]
        // Liên kết tới HoaDon
        public string HoaDonId { get; set; }
        public virtual HoaDon HoaDon { get; set; }

        [ForeignKey("SanPham")]
        // Liên kết tới SanPham
        public int SanPhamId { get; set; }
        public virtual SanPham SanPham { get; set; }
    }
}
