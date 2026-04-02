namespace cuahang.Models
{
    public class KhuyenMai
    {
        public int Id { get; set; }
        public string KMName { get; set; } // Mã code: ví dụ "GIAM30"

        public decimal KMDieuKien { get; set; } // Số tiền tối thiểu để áp dụng (VD: 30,000,000)
        public int KMSoluong { get; set; } // Số lượng mã còn lại trong kho

        public int HeSoGiam { get; set; } // Phần trăm giảm (VD: 15 tương đương 15%)
        public decimal GiamToiDa { get; set; } // Số tiền giảm tối đa (VD: 10,000,000)

        public DateTime NgayHetHang { get; set; }
    }
}