using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cuahang.Migrations
{
    /// <inheritdoc />
    public partial class AddLoaiSptoSanPham : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LoaiSp",
                table: "SanPham",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LoaiSp",
                table: "SanPham");
        }
    }
}
