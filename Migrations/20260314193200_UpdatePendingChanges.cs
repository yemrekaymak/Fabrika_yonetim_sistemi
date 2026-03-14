using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FabrikaBackend.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePendingChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Expenses",
                columns: table => new
                {
                    GiderAdi = table.Column<string>(type: "TEXT", nullable: false),
                    Tutar = table.Column<double>(type: "REAL", nullable: false),
                    GiderTipi = table.Column<string>(type: "TEXT", nullable: false),
                    Tarih = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CompanyId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Expenses", x => x.GiderAdi);
                });

            migrationBuilder.CreateTable(
                name: "Incomes",
                columns: table => new
                {
                    GelirAdi = table.Column<string>(type: "TEXT", nullable: false),
                    Tutar = table.Column<double>(type: "REAL", nullable: false),
                    Tarih = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CompanyId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Incomes", x => x.GelirAdi);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Expenses");

            migrationBuilder.DropTable(
                name: "Incomes");
        }
    }
}
