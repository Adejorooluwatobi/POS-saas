using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POS.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class CustomerRegistrationTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSelfRegistered",
                table: "Customers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "RegisteredByStaffId",
                table: "Customers",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RegisteredStoreId",
                table: "Customers",
                type: "uuid",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Tenants",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                column: "UpdatedAt",
                value: new DateTimeOffset(new DateTime(2026, 9, 20, 19, 36, 49, 65, DateTimeKind.Unspecified).AddTicks(1405), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.CreateIndex(
                name: "IX_Customers_IsSelfRegistered",
                table: "Customers",
                column: "IsSelfRegistered");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_RegisteredByStaffId",
                table: "Customers",
                column: "RegisteredByStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_RegisteredStoreId",
                table: "Customers",
                column: "RegisteredStoreId");

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_Staff_RegisteredByStaffId",
                table: "Customers",
                column: "RegisteredByStaffId",
                principalTable: "Staff",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_Stores_RegisteredStoreId",
                table: "Customers",
                column: "RegisteredStoreId",
                principalTable: "Stores",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Customers_Staff_RegisteredByStaffId",
                table: "Customers");

            migrationBuilder.DropForeignKey(
                name: "FK_Customers_Stores_RegisteredStoreId",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_IsSelfRegistered",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_RegisteredByStaffId",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_RegisteredStoreId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "IsSelfRegistered",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "RegisteredByStaffId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "RegisteredStoreId",
                table: "Customers");

            migrationBuilder.UpdateData(
                table: "Tenants",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                column: "UpdatedAt",
                value: new DateTimeOffset(new DateTime(2026, 9, 20, 16, 24, 53, 711, DateTimeKind.Unspecified).AddTicks(3911), new TimeSpan(0, 0, 0, 0, 0)));
        }
    }
}
