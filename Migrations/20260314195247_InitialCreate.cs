using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FabrikaBackend.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    FullName = table.Column<string>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    Phone = table.Column<string>(type: "TEXT", nullable: false),
                    CompanyName = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                });

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

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProductId = table.Column<int>(type: "INTEGER", nullable: false),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    EstimatedDays = table.Column<double>(type: "REAL", nullable: false),
                    TotalCost = table.Column<double>(type: "REAL", nullable: false),
                    SalePrice = table.Column<double>(type: "REAL", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Personnels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CompanyId = table.Column<int>(type: "INTEGER", nullable: false),
                    FirstName = table.Column<string>(type: "TEXT", nullable: false),
                    LastName = table.Column<string>(type: "TEXT", nullable: false),
                    TcNo = table.Column<string>(type: "TEXT", maxLength: 11, nullable: false),
                    PhoneNumber = table.Column<string>(type: "TEXT", maxLength: 11, nullable: false),
                    Department = table.Column<string>(type: "TEXT", nullable: false),
                    Position = table.Column<string>(type: "TEXT", nullable: false),
                    Salary = table.Column<decimal>(type: "TEXT", nullable: false),
                    HireDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TotalAnnualLeave = table.Column<int>(type: "INTEGER", nullable: false),
                    UsedLeave = table.Column<int>(type: "INTEGER", nullable: false),
                    RemainingLeave = table.Column<int>(type: "INTEGER", nullable: false),
                    PerformanceScore = table.Column<double>(type: "REAL", nullable: false),
                    AverageDailyProduction = table.Column<double>(type: "REAL", nullable: false),
                    AbsenteeismDays = table.Column<int>(type: "INTEGER", nullable: false),
                    OvertimeHours = table.Column<double>(type: "REAL", nullable: false),
                    Certifications = table.Column<string>(type: "TEXT", nullable: false),
                    EmergencyContactName = table.Column<string>(type: "TEXT", nullable: false),
                    EmergencyContactPhone = table.Column<string>(type: "TEXT", maxLength: 11, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Personnels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UrunKodu = table.Column<string>(type: "TEXT", nullable: false),
                    HamMadde = table.Column<string>(type: "TEXT", nullable: false),
                    MalzemeTipi = table.Column<string>(type: "TEXT", nullable: false),
                    PresKategorisi = table.Column<string>(type: "TEXT", nullable: false),
                    GunlukUretim = table.Column<int>(type: "INTEGER", nullable: false),
                    BrutAgirlikKg = table.Column<double>(type: "REAL", nullable: false),
                    NetAgirlikKg = table.Column<double>(type: "REAL", nullable: false),
                    HurdaOrani = table.Column<double>(type: "REAL", nullable: false),
                    MalzemeVerimi = table.Column<double>(type: "REAL", nullable: false),
                    CalisanSayisi = table.Column<int>(type: "INTEGER", nullable: false),
                    HasHeatTreatment = table.Column<bool>(type: "INTEGER", nullable: false),
                    BaseCost = table.Column<double>(type: "REAL", nullable: false),
                    CurrentStock = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Stocks",
                columns: table => new
                {
                    Code = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    QuantityText = table.Column<string>(type: "TEXT", nullable: false),
                    Quantity = table.Column<double>(type: "REAL", nullable: false),
                    Capacity = table.Column<double>(type: "REAL", nullable: false),
                    CriticalLevel = table.Column<double>(type: "REAL", nullable: false),
                    UnitCost = table.Column<decimal>(type: "TEXT", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stocks", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    Password = table.Column<string>(type: "TEXT", nullable: false),
                    Role = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Machines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProductId = table.Column<int>(type: "INTEGER", nullable: false),
                    MachineName = table.Column<string>(type: "TEXT", nullable: false),
                    IsUsed = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Machines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Machines_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Machines_ProductId",
                table: "Machines",
                column: "ProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropTable(
                name: "Expenses");

            migrationBuilder.DropTable(
                name: "Incomes");

            migrationBuilder.DropTable(
                name: "Machines");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "Personnels");

            migrationBuilder.DropTable(
                name: "Stocks");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Products");
        }
    }
}
