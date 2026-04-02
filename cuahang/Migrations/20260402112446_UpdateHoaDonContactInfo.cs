using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cuahang.Migrations
{
    /// <inheritdoc />
    public partial class UpdateHoaDonContactInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DiaChi",
                table: "HoaDon",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "GiamGia",
                table: "HoaDon",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "SoDienThoai",
                table: "HoaDon",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiaChi",
                table: "HoaDon");

            migrationBuilder.DropColumn(
                name: "GiamGia",
                table: "HoaDon");

            migrationBuilder.DropColumn(
                name: "SoDienThoai",
                table: "HoaDon");
        }
    }
}
