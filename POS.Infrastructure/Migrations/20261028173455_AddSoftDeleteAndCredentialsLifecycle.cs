using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteAndCredentialsLifecycle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tenants_ContactEmail",
                table: "Tenants");

            migrationBuilder.DropIndex(
                name: "IX_Tenants_Slug",
                table: "Tenants");

            migrationBuilder.DropIndex(
                name: "IX_Staff_TenantId_Email",
                table: "Staff");

            migrationBuilder.DropIndex(
                name: "IX_Staff_TenantId_EmployeeNo",
                table: "Staff");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                table: "Tenants",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Tenants",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                table: "Staff",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Staff",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "MustChangePassword",
                table: "Staff",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "MustChangePin",
                table: "Staff",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Staff",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000002"),
                column: "DeletedAt",
                value: null);

            migrationBuilder.UpdateData(
                table: "Tenants",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                columns: new[] { "DeletedAt", "UpdatedAt" },
                values: new object[] { null, new DateTimeOffset(new DateTime(2026, 10, 28, 17, 34, 52, 757, DateTimeKind.Unspecified).AddTicks(7365), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_ContactEmail",
                table: "Tenants",
                column: "ContactEmail",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_IsDeleted",
                table: "Tenants",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_Slug",
                table: "Tenants",
                column: "Slug",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Staff_IsDeleted",
                table: "Staff",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Staff_TenantId_Email",
                table: "Staff",
                columns: new[] { "TenantId", "Email" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Staff_TenantId_EmployeeNo",
                table: "Staff",
                columns: new[] { "TenantId", "EmployeeNo" },
                unique: true,
                filter: "\"IsDeleted\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tenants_ContactEmail",
                table: "Tenants");

            migrationBuilder.DropIndex(
                name: "IX_Tenants_IsDeleted",
                table: "Tenants");

            migrationBuilder.DropIndex(
                name: "IX_Tenants_Slug",
                table: "Tenants");

            migrationBuilder.DropIndex(
                name: "IX_Staff_IsDeleted",
                table: "Staff");

            migrationBuilder.DropIndex(
                name: "IX_Staff_TenantId_Email",
                table: "Staff");

            migrationBuilder.DropIndex(
                name: "IX_Staff_TenantId_EmployeeNo",
                table: "Staff");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Staff");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Staff");

            migrationBuilder.DropColumn(
                name: "MustChangePassword",
                table: "Staff");

            migrationBuilder.DropColumn(
                name: "MustChangePin",
                table: "Staff");

            migrationBuilder.UpdateData(
                table: "Tenants",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                column: "UpdatedAt",
                value: new DateTimeOffset(new DateTime(2026, 10, 26, 19, 55, 21, 734, DateTimeKind.Unspecified).AddTicks(131), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_ContactEmail",
                table: "Tenants",
                column: "ContactEmail",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_Slug",
                table: "Tenants",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Staff_TenantId_Email",
                table: "Staff",
                columns: new[] { "TenantId", "Email" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Staff_TenantId_EmployeeNo",
                table: "Staff",
                columns: new[] { "TenantId", "EmployeeNo" },
                unique: true);
        }
    }
}
