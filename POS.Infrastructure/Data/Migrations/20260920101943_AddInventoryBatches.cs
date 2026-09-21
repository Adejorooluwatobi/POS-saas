using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POS.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddInventoryBatches : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BatchNumber",
                table: "InventoryOrderItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ExpiryDate",
                table: "InventoryOrderItems",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ProductionDate",
                table: "InventoryOrderItems",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "InventoryBatches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    InventoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    VariantId = table.Column<Guid>(type: "uuid", nullable: false),
                    StoreId = table.Column<Guid>(type: "uuid", nullable: false),
                    BatchNumber = table.Column<string>(type: "text", nullable: false),
                    ProductionDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ExpiryDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    QuantityOnHand = table.Column<int>(type: "integer", nullable: false),
                    QuantityReserved = table.Column<int>(type: "integer", nullable: false),
                    ExpiryAlertPercentage = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryBatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryBatches_Inventories_InventoryId",
                        column: x => x.InventoryId,
                        principalTable: "Inventories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InventoryBatches_ProductVariants_VariantId",
                        column: x => x.VariantId,
                        principalTable: "ProductVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InventoryBatches_Stores_StoreId",
                        column: x => x.StoreId,
                        principalTable: "Stores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InventoryBatches_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Tenants",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                column: "UpdatedAt",
                value: new DateTimeOffset(new DateTime(2026, 9, 20, 10, 19, 37, 817, DateTimeKind.Unspecified).AddTicks(4081), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.CreateIndex(
                name: "IX_InventoryBatches_InventoryId",
                table: "InventoryBatches",
                column: "InventoryId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryBatches_StoreId",
                table: "InventoryBatches",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryBatches_TenantId",
                table: "InventoryBatches",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryBatches_VariantId",
                table: "InventoryBatches",
                column: "VariantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InventoryBatches");

            migrationBuilder.DropColumn(
                name: "BatchNumber",
                table: "InventoryOrderItems");

            migrationBuilder.DropColumn(
                name: "ExpiryDate",
                table: "InventoryOrderItems");

            migrationBuilder.DropColumn(
                name: "ProductionDate",
                table: "InventoryOrderItems");

            migrationBuilder.UpdateData(
                table: "Tenants",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                column: "UpdatedAt",
                value: new DateTimeOffset(new DateTime(2026, 9, 17, 8, 34, 54, 934, DateTimeKind.Unspecified).AddTicks(7778), new TimeSpan(0, 0, 0, 0, 0)));
        }
    }
}
