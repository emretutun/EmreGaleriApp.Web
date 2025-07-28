using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EmreGaleriApp.Web.Migrations
{
    /// <inheritdoc />
    public partial class LicenseRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.CreateTable(
                name: "CarLicenseTypes",
                columns: table => new
                {
                    CarId = table.Column<int>(type: "int", nullable: false),
                    LicenseTypeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarLicenseTypes", x => new { x.CarId, x.LicenseTypeId });
                    table.ForeignKey(
                        name: "FK_CarLicenseTypes_Cars_CarId",
                        column: x => x.CarId,
                        principalTable: "Cars",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CarLicenseTypes_LicenseTypes_LicenseTypeId",
                        column: x => x.LicenseTypeId,
                        principalTable: "LicenseTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CarLicenseTypes_LicenseTypeId",
                table: "CarLicenseTypes",
                column: "LicenseTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CarLicenseTypes");

            migrationBuilder.InsertData(
                table: "Cars",
                columns: new[] { "Id", "Brand", "Color", "DailyPrice", "Description", "FuelType", "ImageUrl", "IsAvailable", "Mileage", "Model", "ModelYear" },
                values: new object[,]
                {
                    { 1, "Audi", "Siyah", 1500m, "Siyah Audi A4 - Otomatik Vites", "Benzin", "/images/audia4siyah.jpg", true, 125000, "A4", 2020 },
                    { 2, "BMW", "Gri", 1700m, "Gri BMW 320i - Dizel Motor", "Dizel", "/images/bmw320igri.jpg", true, 105000, "320i", 2019 },
                    { 3, "Fiat", "Gri", 1000m, "Gri Fiat Egea - Manuel Vites", "LPG", "/images/fiategeagri.jpg", true, 95000, "Egea", 2021 },
                    { 4, "Mercedes", "Beyaz", 1800m, "Beyaz Mercedes C180 - Lüks Konfor", "Benzin", "/images/mercedesc180beyaz.jpg", true, 85000, "C180", 2022 },
                    { 5, "Toyota", "Beyaz", 1200m, "Beyaz Toyota Corolla - Ekonomik Seçenek", "Benzin", "/images/toyotacorollabeyaz.jpg", true, 90000, "Corolla", 2020 }
                });
        }
    }
}
