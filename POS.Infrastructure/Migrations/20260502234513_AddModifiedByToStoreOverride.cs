using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddModifiedByToStoreOverride : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StoreProductOverrides_StoreId",
                table: "StoreProductOverrides");

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "StoreProductOverrides",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Tenants",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                column: "UpdatedAt",
                value: new DateTimeOffset(new DateTime(2026, 5, 2, 23, 45, 7, 709, DateTimeKind.Unspecified).AddTicks(2253), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.CreateIndex(
                name: "IX_StoreProductOverrides_StoreId_ProductId",
                table: "StoreProductOverrides",
                columns: new[] { "StoreId", "ProductId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StoreProductOverrides_StoreId_ProductId",
                table: "StoreProductOverrides");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "StoreProductOverrides");

            migrationBuilder.UpdateData(
                table: "Tenants",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                column: "UpdatedAt",
                value: new DateTimeOffset(new DateTime(2026, 5, 2, 22, 1, 1, 466, DateTimeKind.Unspecified).AddTicks(9317), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.CreateIndex(
                name: "IX_StoreProductOverrides_StoreId",
                table: "StoreProductOverrides",
                column: "StoreId");
        }
    }
}
