namespace cuahang.Models 
{
    public class CartItem
    {
        public int SanPhamId { get; set; }
        public string? TenSP { get; set; }
        public string? ImageUrl { get; set; }
        public decimal Gia { get; set; }
        public int SoLuong { get; set; }

        // Thuộc tính tính toán nhanh tổng tiền của 1 dòng sản phẩm
        public decimal ThanhTien => SoLuong * Gia;
    }
}