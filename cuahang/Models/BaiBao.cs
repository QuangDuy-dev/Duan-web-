using System;
namespace cuahang.Models
{
    public class BaiBao
    {
        public int Id { get; set; }

        public string TieuDe { get; set; } = string.Empty;
        public string NoiDung { get; set; } 
        public string? HinhAnh { get; set; } = "0";

        public DateTime? NgayDang { get; set; } = DateTime.Now;
    }
}
