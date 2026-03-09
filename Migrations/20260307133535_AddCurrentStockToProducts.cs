using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FabrikaBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddCurrentStockToProducts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Varsa eski tabloyu uçuruyoruz
            migrationBuilder.DropTable(
                name: "StockTransactions");

            // 2. Yeni stok kolonunu ürünler tablosuna monte ediyoruz
            migrationBuilder.AddColumn<int>(
                name: "CurrentStock",
                table: "Products",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            // 3. EFSANE KISIM: SQL TRIGGER EKLEME
            // Sipariş 'Completed' olduğu an bu SQL devreye girip stoku artıracak.
            migrationBuilder.Sql(@"
                CREATE TRIGGER IF NOT EXISTS TRG_UpdateStockOnCompletion
                AFTER UPDATE ON Orders
                FOR EACH ROW
                WHEN NEW.Status = 'Completed' AND OLD.Status != 'Completed'
                BEGIN
                    UPDATE Products 
                    SET CurrentStock = CurrentStock + NEW.Quantity
                    WHERE Id = NEW.ProductId;
                END;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Geri alma (Undo) durumunda tetikleyiciyi sil
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS TRG_UpdateStockOnCompletion;");

            migrationBuilder.DropColumn(
                name: "CurrentStock",
                table: "Products");

            migrationBuilder.CreateTable(
                name: "StockTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ProductId = table.Column<string>(type: "TEXT", nullable: false),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockTransactions", x => x.Id);
                });
        }
    }
}