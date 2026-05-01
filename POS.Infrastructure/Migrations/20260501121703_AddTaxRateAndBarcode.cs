using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTaxRateAndBarcode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "TaxRate",
                table: "Products",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.UpdateData(
                table: "Tenants",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                column: "UpdatedAt",
                value: new DateTimeOffset(new DateTime(2026, 5, 1, 12, 16, 59, 208, DateTimeKind.Unspecified).AddTicks(5863), new TimeSpan(0, 0, 0, 0, 0)));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TaxRate",
                table: "Products");

            migrationBuilder.UpdateData(
                table: "Tenants",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                column: "UpdatedAt",
                value: new DateTimeOffset(new DateTime(2026, 4, 28, 0, 19, 42, 414, DateTimeKind.Unspecified).AddTicks(7760), new TimeSpan(0, 0, 0, 0, 0)));
        }
    }
}
