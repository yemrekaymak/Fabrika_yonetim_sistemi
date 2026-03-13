using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FabrikaBackend.Migrations
{
    /// <inheritdoc />
    public partial class HerSeySifirlandi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FullName = table.Column<string>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    Phone = table.Column<string>(type: "TEXT", nullable: false),
                    CompanyName = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
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
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MusteriAdi = table.Column<string>(type: "TEXT", nullable: false),
                    UrunAdi = table.Column<string>(type: "TEXT", nullable: false),
                    Miktar = table.Column<int>(type: "INTEGER", nullable: false),
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
                    UrunAdi = table.Column<string>(type: "TEXT", nullable: false),
                    HamMaddeTuru = table.Column<string>(type: "TEXT", nullable: false),
                    BirimUretimSuresi = table.Column<int>(type: "INTEGER", nullable: false),
                    SureBirimi = table.Column<string>(type: "TEXT", nullable: false),
                    BrutAgirlikKg = table.Column<int>(type: "INTEGER", nullable: false),
                    NetAgirlikKg = table.Column<int>(type: "INTEGER", nullable: false),
                    HurdaOraniYuzde = table.Column<int>(type: "INTEGER", nullable: false),
                    GunlukUretimKapasitesi = table.Column<int>(type: "INTEGER", nullable: false),
                    SeciliMakineler = table.Column<string>(type: "TEXT", nullable: false),
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
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Quantity = table.Column<double>(type: "REAL", nullable: false),
                    Capacity = table.Column<double>(type: "REAL", nullable: false),
                    CriticalLevel = table.Column<double>(type: "REAL", nullable: false),
                    UnitCost = table.Column<decimal>(type: "TEXT", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stocks", x => x.Id);
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropTable(
                name: "Machines");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "Personnels");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Stocks");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
