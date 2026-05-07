using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPackagingRatios : Migration
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
                .Annotation("Npgsql:Enum:payment_method", "cash,card,mobile_money,bank_transfer,gift_card,store_credit,split")
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

            migrationBuilder.AddColumn<int>(
                name: "RollsPerPack",
                table: "Products",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SinglesPerRoll",
                table: "Products",
                type: "integer",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Tenants",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                column: "UpdatedAt",
                value: new DateTimeOffset(new DateTime(2026, 5, 7, 13, 28, 33, 70, DateTimeKind.Unspecified).AddTicks(5624), new TimeSpan(0, 0, 0, 0, 0)));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RollsPerPack",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SinglesPerRoll",
                table: "Products");

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
                .OldAnnotation("Npgsql:Enum:inventory_order_status", "draft,dispatched,received,approved,disputed,resolved")
                .OldAnnotation("Npgsql:Enum:inventory_order_type", "hq_to_store,store_to_store")
                .OldAnnotation("Npgsql:Enum:payment_method", "cash,card,mobile_money,bank_transfer,gift_card,store_credit,split")
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
                value: new DateTimeOffset(new DateTime(2026, 5, 4, 21, 50, 28, 705, DateTimeKind.Unspecified).AddTicks(1998), new TimeSpan(0, 0, 0, 0, 0)));
        }
    }
}
