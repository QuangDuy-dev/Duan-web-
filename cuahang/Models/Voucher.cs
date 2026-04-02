namespace cuahang.Models
{
    public class Voucher
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty; // Mã code ví dụ: GIAM20
        public decimal DiscountValue { get; set; }       // Giá trị giảm
        public bool IsPercentage { get; set; }          // Giảm theo % hay tiền mặt
        public decimal MinOrderValue { get; set; }      // Đơn hàng tối thiểu để áp dụng
        public bool IsActive { get; set; } = true;      // Trạng thái mã
    }
}