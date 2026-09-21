using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POS.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddQuantityReceivedBaseUnits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "QuantityReceivedBaseUnits",
                table: "InventoryOrderItems",
                type: "integer",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Tenants",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                column: "UpdatedAt",
                value: new DateTimeOffset(new DateTime(2026, 9, 17, 8, 34, 54, 934, DateTimeKind.Unspecified).AddTicks(7778), new TimeSpan(0, 0, 0, 0, 0)));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QuantityReceivedBaseUnits",
                table: "InventoryOrderItems");

            migrationBuilder.UpdateData(
                table: "Tenants",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                column: "UpdatedAt",
                value: new DateTimeOffset(new DateTime(2026, 9, 16, 19, 19, 12, 292, DateTimeKind.Unspecified).AddTicks(2919), new TimeSpan(0, 0, 0, 0, 0)));
        }
    }
}
