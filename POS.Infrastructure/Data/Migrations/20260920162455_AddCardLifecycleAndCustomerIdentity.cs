using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POS.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCardLifecycleAndCustomerIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "GiftCards",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EncryptedIdentityNumber",
                table: "Customers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdentityType",
                table: "Customers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsIdentityVerified",
                table: "Customers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LivenessAuditLog",
                table: "Customers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LivenessVerifiedAt",
                table: "Customers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhotoUrl",
                table: "Customers",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Tenants",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                column: "UpdatedAt",
                value: new DateTimeOffset(new DateTime(2026, 9, 20, 16, 24, 53, 711, DateTimeKind.Unspecified).AddTicks(3911), new TimeSpan(0, 0, 0, 0, 0)));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Notes",
                table: "GiftCards");

            migrationBuilder.DropColumn(
                name: "EncryptedIdentityNumber",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "IdentityType",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "IsIdentityVerified",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "LivenessAuditLog",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "LivenessVerifiedAt",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "PhotoUrl",
                table: "Customers");

            migrationBuilder.UpdateData(
                table: "Tenants",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                column: "UpdatedAt",
                value: new DateTimeOffset(new DateTime(2026, 9, 20, 14, 44, 26, 696, DateTimeKind.Unspecified).AddTicks(2839), new TimeSpan(0, 0, 0, 0, 0)));
        }
    }
}
