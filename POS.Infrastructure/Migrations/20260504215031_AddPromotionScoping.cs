using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPromotionScoping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Promotions",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "StoreId",
                table: "Promotions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "IssuingStoreId",
                table: "GiftCards",
                type: "uuid",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Tenants",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                column: "UpdatedAt",
                value: new DateTimeOffset(new DateTime(2026, 5, 4, 21, 50, 28, 705, DateTimeKind.Unspecified).AddTicks(1998), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.CreateIndex(
                name: "IX_Promotions_Code",
                table: "Promotions",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_Promotions_StoreId",
                table: "Promotions",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_GiftCards_IssuingStoreId",
                table: "GiftCards",
                column: "IssuingStoreId");

            migrationBuilder.AddForeignKey(
                name: "FK_GiftCards_Stores_IssuingStoreId",
                table: "GiftCards",
                column: "IssuingStoreId",
                principalTable: "Stores",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Promotions_Stores_StoreId",
                table: "Promotions",
                column: "StoreId",
                principalTable: "Stores",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GiftCards_Stores_IssuingStoreId",
                table: "GiftCards");

            migrationBuilder.DropForeignKey(
                name: "FK_Promotions_Stores_StoreId",
                table: "Promotions");

            migrationBuilder.DropIndex(
                name: "IX_Promotions_Code",
                table: "Promotions");

            migrationBuilder.DropIndex(
                name: "IX_Promotions_StoreId",
                table: "Promotions");

            migrationBuilder.DropIndex(
                name: "IX_GiftCards_IssuingStoreId",
                table: "GiftCards");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Promotions");

            migrationBuilder.DropColumn(
                name: "StoreId",
                table: "Promotions");

            migrationBuilder.DropColumn(
                name: "IssuingStoreId",
                table: "GiftCards");

            migrationBuilder.UpdateData(
                table: "Tenants",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                column: "UpdatedAt",
                value: new DateTimeOffset(new DateTime(2026, 5, 3, 14, 51, 45, 575, DateTimeKind.Unspecified).AddTicks(1686), new TimeSpan(0, 0, 0, 0, 0)));
        }
    }
}
