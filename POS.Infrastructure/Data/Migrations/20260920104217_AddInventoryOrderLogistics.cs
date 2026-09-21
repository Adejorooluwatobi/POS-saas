using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POS.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddInventoryOrderLogistics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DriverName",
                table: "InventoryOrders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DriverPhone",
                table: "InventoryOrders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "EstimatedDeliveryTime",
                table: "InventoryOrders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VehiclePlateNumber",
                table: "InventoryOrders",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Tenants",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                column: "UpdatedAt",
                value: new DateTimeOffset(new DateTime(2026, 9, 20, 10, 42, 13, 84, DateTimeKind.Unspecified).AddTicks(5202), new TimeSpan(0, 0, 0, 0, 0)));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DriverName",
                table: "InventoryOrders");

            migrationBuilder.DropColumn(
                name: "DriverPhone",
                table: "InventoryOrders");

            migrationBuilder.DropColumn(
                name: "EstimatedDeliveryTime",
                table: "InventoryOrders");

            migrationBuilder.DropColumn(
                name: "VehiclePlateNumber",
                table: "InventoryOrders");

            migrationBuilder.UpdateData(
                table: "Tenants",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                column: "UpdatedAt",
                value: new DateTimeOffset(new DateTime(2026, 9, 20, 10, 19, 37, 817, DateTimeKind.Unspecified).AddTicks(4081), new TimeSpan(0, 0, 0, 0, 0)));
        }
    }
}
