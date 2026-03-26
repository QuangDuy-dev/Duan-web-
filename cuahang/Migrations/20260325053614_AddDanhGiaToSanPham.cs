using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cuahang.Migrations
{
    /// <inheritdoc />
    public partial class AddDanhGiaToSanPham : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "HinhAnh",
                table: "SanPham",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "DanhGia",
                table: "SanPham",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DanhGia",
                table: "SanPham");

            migrationBuilder.AlterColumn<string>(
                name: "HinhAnh",
                table: "SanPham",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
