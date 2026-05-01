using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTerminalPairing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeviceToken",
                table: "Terminals",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PairingCode",
                table: "Terminals",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "PairingCodeExpiresAt",
                table: "Terminals",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Tenants",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                column: "UpdatedAt",
                value: new DateTimeOffset(new DateTime(2026, 5, 1, 13, 36, 12, 497, DateTimeKind.Unspecified).AddTicks(6783), new TimeSpan(0, 0, 0, 0, 0)));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeviceToken",
                table: "Terminals");

            migrationBuilder.DropColumn(
                name: "PairingCode",
                table: "Terminals");

            migrationBuilder.DropColumn(
                name: "PairingCodeExpiresAt",
                table: "Terminals");

            migrationBuilder.UpdateData(
                table: "Tenants",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                column: "UpdatedAt",
                value: new DateTimeOffset(new DateTime(2026, 5, 1, 12, 16, 59, 208, DateTimeKind.Unspecified).AddTicks(5863), new TimeSpan(0, 0, 0, 0, 0)));
        }
    }
}
