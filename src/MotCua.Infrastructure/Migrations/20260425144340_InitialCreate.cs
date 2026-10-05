using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MotCua.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "YeuCauHoiDaps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaSinhVien = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    NoiDung = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    TepDinhKem = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: true),
                    MaYeuCau = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TrangThai = table.Column<int>(type: "int", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YeuCauHoiDaps", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TinNhans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    YeuCauId = table.Column<int>(type: "int", nullable: false),
                    ToEmail = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DaGui = table.Column<bool>(type: "bit", nullable: false),
                    TaoLuc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GuiLuc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LoiGui = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SoLanThu = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TinNhans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TinNhans_YeuCauHoiDaps_YeuCauId",
                        column: x => x.YeuCauId,
                        principalTable: "YeuCauHoiDaps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TinNhans_DaGui_TaoLuc",
                table: "TinNhans",
                columns: new[] { "DaGui", "TaoLuc" });

            migrationBuilder.CreateIndex(
                name: "IX_TinNhans_YeuCauId",
                table: "TinNhans",
                column: "YeuCauId");

            migrationBuilder.CreateIndex(
                name: "IX_YeuCauHoiDaps_MaSinhVien",
                table: "YeuCauHoiDaps",
                column: "MaSinhVien");

            migrationBuilder.CreateIndex(
                name: "IX_YeuCauHoiDaps_MaYeuCau",
                table: "YeuCauHoiDaps",
                column: "MaYeuCau",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TinNhans");

            migrationBuilder.DropTable(
                name: "YeuCauHoiDaps");
        }
    }
}