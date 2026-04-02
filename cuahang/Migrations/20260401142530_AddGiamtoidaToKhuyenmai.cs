using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cuahang.Migrations
{
    /// <inheritdoc />
    public partial class AddGiamtoidaToKhuyenmai : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GiamToiDa",
                table: "khuyenMais",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GiamToiDa",
                table: "khuyenMais");
        }
    }
}
