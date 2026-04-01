namespace cuahang.Models
{
    public class KhuyenMai
    {
        public int Id { get; set; }
        public string KMName { get; set; }
        public string KMDieuKien { get; set; }

        public int KMSoluong { get; set; }
        public string HeSoGiam {  get; set; }

        public string GiamToiDa { get; set; }

        public DateTime NgayHetHang { get; set; }
    }
}
