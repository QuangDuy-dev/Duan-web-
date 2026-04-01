using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cuahang.Migrations
{
    /// <inheritdoc />
    public partial class NewTableKM : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "NgayDang",
                table: "BaiBao",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "HinhAnh",
                table: "BaiBao",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateTable(
                name: "khuyenMais",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KMName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KMDieuKien = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KMSoluong = table.Column<int>(type: "int", nullable: false),
                    HeSoGiam = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgayHetHang = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_khuyenMais", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "khuyenMais");

            migrationBuilder.AlterColumn<DateTime>(
                name: "NgayDang",
                table: "BaiBao",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "HinhAnh",
                table: "BaiBao",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
