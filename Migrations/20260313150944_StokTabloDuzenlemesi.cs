using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FabrikaBackend.Migrations
{
    /// <inheritdoc />
    public partial class StokTabloDuzenlemesi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BirimUretimSuresi",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "BrutAgirlikKg",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "GunlukUretimKapasitesi",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "HamMaddeTuru",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SeciliMakineler",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SureBirimi",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "NetAgirlikKg",
                table: "Products",
                newName: "KritikSeviye");

            migrationBuilder.RenameColumn(
                name: "HurdaOraniYuzde",
                table: "Products",
                newName: "Kapasite");

            migrationBuilder.AddColumn<double>(
                name: "BrutAgirlik",
                table: "Products",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Fiyat",
                table: "Products",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "HurdaOrani",
                table: "Products",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Maliyet",
                table: "Products",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "NetAgirlik",
                table: "Products",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BrutAgirlik",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Fiyat",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "HurdaOrani",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Maliyet",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "NetAgirlik",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "KritikSeviye",
                table: "Products",
                newName: "NetAgirlikKg");

            migrationBuilder.RenameColumn(
                name: "Kapasite",
                table: "Products",
                newName: "HurdaOraniYuzde");

            migrationBuilder.AddColumn<int>(
                name: "BirimUretimSuresi",
                table: "Products",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BrutAgirlikKg",
                table: "Products",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GunlukUretimKapasitesi",
                table: "Products",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "HamMaddeTuru",
                table: "Products",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SeciliMakineler",
                table: "Products",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SureBirimi",
                table: "Products",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
