using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POS.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerCardsAndLoyaltySettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LoyaltyMinRedemptionPoints",
                table: "Tenants",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "LoyaltyPointRedeemRate",
                table: "Tenants",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "LoyaltyPointsEarnRate",
                table: "Tenants",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "LoyaltyProgramEnabled",
                table: "Tenants",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                table: "GiftCards",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "GiftCardTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    GiftCardId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    BalanceBefore = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    BalanceAfter = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    Method = table.Column<int>(type: "integer", nullable: false),
                    Reference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    StoreId = table.Column<Guid>(type: "uuid", nullable: true),
                    StaffId = table.Column<Guid>(type: "uuid", nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GiftCardTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GiftCardTransactions_GiftCards_GiftCardId",
                        column: x => x.GiftCardId,
                        principalTable: "GiftCards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GiftCardTransactions_Staff_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Staff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_GiftCardTransactions_Stores_StoreId",
                        column: x => x.StoreId,
                        principalTable: "Stores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_GiftCardTransactions_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Tenants",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                columns: new[] { "LoyaltyMinRedemptionPoints", "LoyaltyPointRedeemRate", "LoyaltyPointsEarnRate", "LoyaltyProgramEnabled", "UpdatedAt" },
                values: new object[] { 50, 1m, 100m, true, new DateTimeOffset(new DateTime(2026, 9, 20, 14, 44, 26, 696, DateTimeKind.Unspecified).AddTicks(2839), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.CreateIndex(
                name: "IX_GiftCards_CustomerId",
                table: "GiftCards",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_GiftCardTransactions_CreatedAt",
                table: "GiftCardTransactions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_GiftCardTransactions_GiftCardId",
                table: "GiftCardTransactions",
                column: "GiftCardId");

            migrationBuilder.CreateIndex(
                name: "IX_GiftCardTransactions_StaffId",
                table: "GiftCardTransactions",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_GiftCardTransactions_StoreId",
                table: "GiftCardTransactions",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_GiftCardTransactions_TenantId",
                table: "GiftCardTransactions",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_GiftCards_Customers_CustomerId",
                table: "GiftCards",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GiftCards_Customers_CustomerId",
                table: "GiftCards");

            migrationBuilder.DropTable(
                name: "GiftCardTransactions");

            migrationBuilder.DropIndex(
                name: "IX_GiftCards_CustomerId",
                table: "GiftCards");

            migrationBuilder.DropColumn(
                name: "LoyaltyMinRedemptionPoints",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "LoyaltyPointRedeemRate",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "LoyaltyPointsEarnRate",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "LoyaltyProgramEnabled",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "GiftCards");

            migrationBuilder.UpdateData(
                table: "Tenants",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                column: "UpdatedAt",
                value: new DateTimeOffset(new DateTime(2026, 9, 20, 10, 42, 13, 84, DateTimeKind.Unspecified).AddTicks(5202), new TimeSpan(0, 0, 0, 0, 0)));
        }
    }
}
