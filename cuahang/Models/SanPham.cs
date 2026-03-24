namespace cuahang.Models
{
    public class SanPham
    {
        public int Id { get; set; }
        public string TenSP { get; set; }
        public decimal Gia { get; set; }
        public string HinhAnh { get; set; }
        public string MoTa { get; set; }
        public int SoLuongTon { get; set; }

        public string? ImageUrl { get; set; }

        // Một sản phẩm có thể nằm trong nhiều chi tiết hóa đơn
        public virtual ICollection<ChiTietHoaDon> ChiTietHoaDons { get; set; }
    }
}
