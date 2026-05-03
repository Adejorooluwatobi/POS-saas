using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInventoryOrderSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:audit_action", "insert,update,delete,login,logout,unauthorized_access")
                .Annotation("Npgsql:Enum:billing_cycle", "monthly,annually")
                .Annotation("Npgsql:Enum:customer_tier", "bronze,silver,gold,platinum")
                .Annotation("Npgsql:Enum:discount_type", "promo,coupon,loyalty,manual,staff")
                .Annotation("Npgsql:Enum:inventory_order_status", "draft,dispatched,received,approved,disputed,resolved")
                .Annotation("Npgsql:Enum:inventory_order_type", "hq_to_store,store_to_store")
                .Annotation("Npgsql:Enum:payment_method", "cash,card,mobile_money,gift_card,store_credit,split")
                .Annotation("Npgsql:Enum:payment_status", "pending,approved,declined,refunded,partially_refunded")
                .Annotation("Npgsql:Enum:promotion_scope", "product,category,cart,customer_tier")
                .Annotation("Npgsql:Enum:promotion_type", "percent,fixed,bogo,bundle,free_item")
                .Annotation("Npgsql:Enum:requisition_status", "pending,under_review,approved,partially_fulfilled,fully_fulfilled,rejected,cancelled")
                .Annotation("Npgsql:Enum:session_status", "open,suspended,closed")
                .Annotation("Npgsql:Enum:subscription_plan", "starter,pro,enterprise")
                .Annotation("Npgsql:Enum:subscription_status", "trial,active,past_due,cancelled,suspended")
                .Annotation("Npgsql:Enum:system_role", "super_admin,tenant_admin,store_manager,cashier,supervisor,manager")
                .Annotation("Npgsql:Enum:tax_category", "standard,zero,exempt,reduced")
                .Annotation("Npgsql:Enum:terminal_status", "online,offline,maintenance")
                .Annotation("Npgsql:Enum:transaction_status", "open,in_progress,payment_pending,completed,voided,refunded")
                .Annotation("Npgsql:Enum:transaction_type", "sale,return,exchange,void")
                .OldAnnotation("Npgsql:Enum:audit_action", "insert,update,delete,login,logout,unauthorized_access")
                .OldAnnotation("Npgsql:Enum:billing_cycle", "monthly,annually")
                .OldAnnotation("Npgsql:Enum:customer_tier", "bronze,silver,gold,platinum")
                .OldAnnotation("Npgsql:Enum:discount_type", "promo,coupon,loyalty,manual,staff")
                .OldAnnotation("Npgsql:Enum:payment_method", "cash,card,mobile_money,gift_card,store_credit,split")
                .OldAnnotation("Npgsql:Enum:payment_status", "pending,approved,declined,refunded,partially_refunded")
                .OldAnnotation("Npgsql:Enum:promotion_scope", "product,category,cart,customer_tier")
                .OldAnnotation("Npgsql:Enum:promotion_type", "percent,fixed,bogo,bundle,free_item")
                .OldAnnotation("Npgsql:Enum:session_status", "open,suspended,closed")
                .OldAnnotation("Npgsql:Enum:subscription_plan", "starter,pro,enterprise")
                .OldAnnotation("Npgsql:Enum:subscription_status", "trial,active,past_due,cancelled,suspended")
                .OldAnnotation("Npgsql:Enum:system_role", "super_admin,tenant_admin,store_manager,cashier,supervisor,manager")
                .OldAnnotation("Npgsql:Enum:tax_category", "standard,zero,exempt,reduced")
                .OldAnnotation("Npgsql:Enum:terminal_status", "online,offline,maintenance")
                .OldAnnotation("Npgsql:Enum:transaction_status", "open,in_progress,payment_pending,completed,voided,refunded")
                .OldAnnotation("Npgsql:Enum:transaction_type", "sale,return,exchange,void");

            migrationBuilder.AddColumn<Guid>(
                name: "BaseVariantId",
                table: "ProductVariants",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ConversionFactor",
                table: "ProductVariants",
                type: "numeric(12,4)",
                precision: 12,
                scale: 4,
                nullable: false,
                defaultValue: 1m);

            migrationBuilder.AddColumn<bool>(
                name: "IsBaseUnit",
                table: "ProductVariants",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateTable(
                name: "StockRequisitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequisitionNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    RequestingStoreId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedByStaffId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReviewedByStaffId = table.Column<Guid>(type: "uuid", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    RejectionReason = table.Column<string>(type: "text", nullable: true),
                    ReviewedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockRequisitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockRequisitions_Staff_CreatedByStaffId",
                        column: x => x.CreatedByStaffId,
                        principalTable: "Staff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockRequisitions_Staff_ReviewedByStaffId",
                        column: x => x.ReviewedByStaffId,
                        principalTable: "Staff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockRequisitions_Stores_RequestingStoreId",
                        column: x => x.RequestingStoreId,
                        principalTable: "Stores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockRequisitions_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InventoryOrders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    SourceStoreId = table.Column<Guid>(type: "uuid", nullable: true),
                    DestinationStoreId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedByStaffId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReceivedByStaffId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedByStaffId = table.Column<Guid>(type: "uuid", nullable: true),
                    ResolvedByStaffId = table.Column<Guid>(type: "uuid", nullable: true),
                    DispatchedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReceivedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ResolvedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    DisputeNotes = table.Column<string>(type: "text", nullable: true),
                    DisputePhotoUrl = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    StockRequisitionId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsReferredTransfer = table.Column<bool>(type: "boolean", nullable: false),
                    ReferralAccepted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryOrders_Staff_ApprovedByStaffId",
                        column: x => x.ApprovedByStaffId,
                        principalTable: "Staff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryOrders_Staff_CreatedByStaffId",
                        column: x => x.CreatedByStaffId,
                        principalTable: "Staff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryOrders_Staff_ReceivedByStaffId",
                        column: x => x.ReceivedByStaffId,
                        principalTable: "Staff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryOrders_Staff_ResolvedByStaffId",
                        column: x => x.ResolvedByStaffId,
                        principalTable: "Staff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryOrders_StockRequisitions_StockRequisitionId",
                        column: x => x.StockRequisitionId,
                        principalTable: "StockRequisitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_InventoryOrders_Stores_DestinationStoreId",
                        column: x => x.DestinationStoreId,
                        principalTable: "Stores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryOrders_Stores_SourceStoreId",
                        column: x => x.SourceStoreId,
                        principalTable: "Stores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryOrders_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StockRequisitionItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StockRequisitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    VariantId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuantityRequested = table.Column<int>(type: "integer", nullable: false),
                    QuantityFulfilled = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockRequisitionItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockRequisitionItems_ProductVariants_VariantId",
                        column: x => x.VariantId,
                        principalTable: "ProductVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockRequisitionItems_StockRequisitions_StockRequisitionId",
                        column: x => x.StockRequisitionId,
                        principalTable: "StockRequisitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InventoryOrderItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InventoryOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    VariantId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuantityOrdered = table.Column<int>(type: "integer", nullable: false),
                    QuantityReceived = table.Column<int>(type: "integer", nullable: true),
                    QuantityDamaged = table.Column<int>(type: "integer", nullable: true),
                    DamageNotes = table.Column<string>(type: "text", nullable: true),
                    DamagePhotoUrl = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryOrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryOrderItems_InventoryOrders_InventoryOrderId",
                        column: x => x.InventoryOrderId,
                        principalTable: "InventoryOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InventoryOrderItems_ProductVariants_VariantId",
                        column: x => x.VariantId,
                        principalTable: "ProductVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "Tenants",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                column: "UpdatedAt",
                value: new DateTimeOffset(new DateTime(2026, 5, 3, 14, 31, 18, 139, DateTimeKind.Unspecified).AddTicks(1664), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariants_BaseVariantId",
                table: "ProductVariants",
                column: "BaseVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryOrderItems_InventoryOrderId",
                table: "InventoryOrderItems",
                column: "InventoryOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryOrderItems_VariantId",
                table: "InventoryOrderItems",
                column: "VariantId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryOrders_ApprovedByStaffId",
                table: "InventoryOrders",
                column: "ApprovedByStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryOrders_CreatedByStaffId",
                table: "InventoryOrders",
                column: "CreatedByStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryOrders_DestinationStoreId",
                table: "InventoryOrders",
                column: "DestinationStoreId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryOrders_OrderNumber",
                table: "InventoryOrders",
                column: "OrderNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryOrders_ReceivedByStaffId",
                table: "InventoryOrders",
                column: "ReceivedByStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryOrders_ResolvedByStaffId",
                table: "InventoryOrders",
                column: "ResolvedByStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryOrders_SourceStoreId",
                table: "InventoryOrders",
                column: "SourceStoreId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryOrders_StockRequisitionId",
                table: "InventoryOrders",
                column: "StockRequisitionId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryOrders_TenantId",
                table: "InventoryOrders",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_StockRequisitionItems_StockRequisitionId",
                table: "StockRequisitionItems",
                column: "StockRequisitionId");

            migrationBuilder.CreateIndex(
                name: "IX_StockRequisitionItems_VariantId",
                table: "StockRequisitionItems",
                column: "VariantId");

            migrationBuilder.CreateIndex(
                name: "IX_StockRequisitions_CreatedByStaffId",
                table: "StockRequisitions",
                column: "CreatedByStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_StockRequisitions_RequestingStoreId",
                table: "StockRequisitions",
                column: "RequestingStoreId");

            migrationBuilder.CreateIndex(
                name: "IX_StockRequisitions_RequisitionNumber",
                table: "StockRequisitions",
                column: "RequisitionNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockRequisitions_ReviewedByStaffId",
                table: "StockRequisitions",
                column: "ReviewedByStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_StockRequisitions_TenantId",
                table: "StockRequisitions",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductVariants_ProductVariants_BaseVariantId",
                table: "ProductVariants",
                column: "BaseVariantId",
                principalTable: "ProductVariants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductVariants_ProductVariants_BaseVariantId",
                table: "ProductVariants");

            migrationBuilder.DropTable(
                name: "InventoryOrderItems");

            migrationBuilder.DropTable(
                name: "StockRequisitionItems");

            migrationBuilder.DropTable(
                name: "InventoryOrders");

            migrationBuilder.DropTable(
                name: "StockRequisitions");

            migrationBuilder.DropIndex(
                name: "IX_ProductVariants_BaseVariantId",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "BaseVariantId",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "ConversionFactor",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "IsBaseUnit",
                table: "ProductVariants");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:audit_action", "insert,update,delete,login,logout,unauthorized_access")
                .Annotation("Npgsql:Enum:billing_cycle", "monthly,annually")
                .Annotation("Npgsql:Enum:customer_tier", "bronze,silver,gold,platinum")
                .Annotation("Npgsql:Enum:discount_type", "promo,coupon,loyalty,manual,staff")
                .Annotation("Npgsql:Enum:payment_method", "cash,card,mobile_money,gift_card,store_credit,split")
                .Annotation("Npgsql:Enum:payment_status", "pending,approved,declined,refunded,partially_refunded")
                .Annotation("Npgsql:Enum:promotion_scope", "product,category,cart,customer_tier")
                .Annotation("Npgsql:Enum:promotion_type", "percent,fixed,bogo,bundle,free_item")
                .Annotation("Npgsql:Enum:session_status", "open,suspended,closed")
                .Annotation("Npgsql:Enum:subscription_plan", "starter,pro,enterprise")
                .Annotation("Npgsql:Enum:subscription_status", "trial,active,past_due,cancelled,suspended")
                .Annotation("Npgsql:Enum:system_role", "super_admin,tenant_admin,store_manager,cashier,supervisor,manager")
                .Annotation("Npgsql:Enum:tax_category", "standard,zero,exempt,reduced")
                .Annotation("Npgsql:Enum:terminal_status", "online,offline,maintenance")
                .Annotation("Npgsql:Enum:transaction_status", "open,in_progress,payment_pending,completed,voided,refunded")
                .Annotation("Npgsql:Enum:transaction_type", "sale,return,exchange,void")
                .OldAnnotation("Npgsql:Enum:audit_action", "insert,update,delete,login,logout,unauthorized_access")
                .OldAnnotation("Npgsql:Enum:billing_cycle", "monthly,annually")
                .OldAnnotation("Npgsql:Enum:customer_tier", "bronze,silver,gold,platinum")
                .OldAnnotation("Npgsql:Enum:discount_type", "promo,coupon,loyalty,manual,staff")
                .OldAnnotation("Npgsql:Enum:inventory_order_status", "draft,dispatched,received,approved,disputed,resolved")
                .OldAnnotation("Npgsql:Enum:inventory_order_type", "hq_to_store,store_to_store")
                .OldAnnotation("Npgsql:Enum:payment_method", "cash,card,mobile_money,gift_card,store_credit,split")
                .OldAnnotation("Npgsql:Enum:payment_status", "pending,approved,declined,refunded,partially_refunded")
                .OldAnnotation("Npgsql:Enum:promotion_scope", "product,category,cart,customer_tier")
                .OldAnnotation("Npgsql:Enum:promotion_type", "percent,fixed,bogo,bundle,free_item")
                .OldAnnotation("Npgsql:Enum:requisition_status", "pending,under_review,approved,partially_fulfilled,fully_fulfilled,rejected,cancelled")
                .OldAnnotation("Npgsql:Enum:session_status", "open,suspended,closed")
                .OldAnnotation("Npgsql:Enum:subscription_plan", "starter,pro,enterprise")
                .OldAnnotation("Npgsql:Enum:subscription_status", "trial,active,past_due,cancelled,suspended")
                .OldAnnotation("Npgsql:Enum:system_role", "super_admin,tenant_admin,store_manager,cashier,supervisor,manager")
                .OldAnnotation("Npgsql:Enum:tax_category", "standard,zero,exempt,reduced")
                .OldAnnotation("Npgsql:Enum:terminal_status", "online,offline,maintenance")
                .OldAnnotation("Npgsql:Enum:transaction_status", "open,in_progress,payment_pending,completed,voided,refunded")
                .OldAnnotation("Npgsql:Enum:transaction_type", "sale,return,exchange,void");

            migrationBuilder.UpdateData(
                table: "Tenants",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                column: "UpdatedAt",
                value: new DateTimeOffset(new DateTime(2026, 5, 2, 23, 45, 7, 709, DateTimeKind.Unspecified).AddTicks(2253), new TimeSpan(0, 0, 0, 0, 0)));
        }
    }
}
