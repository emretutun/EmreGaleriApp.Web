using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EmreGaleriApp.Web.Migrations
{
    /// <inheritdoc />
    public partial class SeedCars : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Cars",
                columns: new[] { "Id", "Brand", "DailyPrice", "Description", "ImageUrl", "IsAvailable", "Model" },
                values: new object[,]
                {
                    { 1, "Audi", 1500m, "Siyah Audi A4 - Otomatik Vites", "/images/audia4siyah.jpg", true, "A4" },
                    { 2, "BMW", 1700m, "Gri BMW 320i - Dizel Motor", "/images/bmw320igri.jpg", true, "320i" },
                    { 3, "Fiat", 1000m, "Gri Fiat Egea - Manuel Vites", "/images/fiategeagri.jpg", true, "Egea" },
                    { 4, "Mercedes", 1800m, "Beyaz Mercedes C180 - Lüks Konfor", "/images/mercedesc180beyaz.jpg", true, "C180" },
                    { 5, "Toyota", 1200m, "Beyaz Toyota Corolla - Ekonomik Seçenek", "/images/toyotacorollabeyaz.jpg", true, "Corolla" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 5);
        }
    }
}
