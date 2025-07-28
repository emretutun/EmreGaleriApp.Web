using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmreGaleriApp.Web.Migrations
{
    /// <inheritdoc />
    public partial class RemoveApprovedDateFromOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_AspNetUsers_ApprovedByAdminId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_ApprovedByAdminId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ApprovedByAdminId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ApprovedDate",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Orders");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApprovedByAdminId",
                table: "Orders",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedDate",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ApprovedByAdminId",
                table: "Orders",
                column: "ApprovedByAdminId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_AspNetUsers_ApprovedByAdminId",
                table: "Orders",
                column: "ApprovedByAdminId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
