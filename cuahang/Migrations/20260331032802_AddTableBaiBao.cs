using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cuahang.Migrations
{
    /// <inheritdoc />
    public partial class AddTableBaiBao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Title",
                table: "BaiBao",
                newName: "TieuDe");

            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "BaiBao",
                newName: "NoiDung");

            migrationBuilder.RenameColumn(
                name: "CreatedDate",
                table: "BaiBao",
                newName: "NgayDang");

            migrationBuilder.RenameColumn(
                name: "Content",
                table: "BaiBao",
                newName: "HinhAnh");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TieuDe",
                table: "BaiBao",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "NoiDung",
                table: "BaiBao",
                newName: "ImageUrl");

            migrationBuilder.RenameColumn(
                name: "NgayDang",
                table: "BaiBao",
                newName: "CreatedDate");

            migrationBuilder.RenameColumn(
                name: "HinhAnh",
                table: "BaiBao",
                newName: "Content");
        }
    }
}
