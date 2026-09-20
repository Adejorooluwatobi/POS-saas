using Microsoft.EntityFrameworkCore;
using POS.Infrastructure.Data;
using POS.Domain.Entities;
using POS.Domain.Enums;
using POS.Domain.Interfaces;
using Serilog;

namespace POS.Api.Extensions;

public static class MigrationExtensions
{
    public static void ApplyMigrations(this IApplicationBuilder app)
    {
        using IServiceScope scope = app.ApplicationServices.CreateScope();
        using RetailOsDbContext dbContext = 
            scope.ServiceProvider.GetRequiredService<RetailOsDbContext>();

        dbContext.Database.SetCommandTimeout(120);
        
        Log.Information("Applying database migrations...");
        dbContext.Database.Migrate();
        Log.Information("Database migrations applied successfully.");
    }

    public static void SeedSuperAdmin(this IApplicationBuilder app)
    {
        using IServiceScope scope = app.ApplicationServices.CreateScope();
        var services = scope.ServiceProvider;
        
        var dbContext = services.GetRequiredService<RetailOsDbContext>();
        var config = services.GetRequiredService<IConfiguration>();
        var passwordService = services.GetRequiredService<IPasswordService>();

        var adminEmail = config["SuperAdmin:Email"];
        var adminPassword = config["SuperAdmin:Password"];

        if (string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminPassword))
        {
            Log.Warning("SuperAdmin credentials not configured. Skipping SuperAdmin seed.");
            return;
        }

        Log.Information("Seeding SuperAdmin user ({Email})...", adminEmail);

        var superAdminId = Guid.Parse("00000000-0000-0000-0000-000000000002");
        var systemTenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        // Use IgnoreQueryFilters because SuperAdmin might be outside the current tenant context
        var admin = dbContext.Staff.IgnoreQueryFilters().FirstOrDefault(s => s.Id == superAdminId);

        var passwordHash = passwordService.Hash(adminPassword);
        var numericPinHash = passwordService.Hash("1234");

        if (admin == null)
        {
            admin = new Staff
            {
                Id = superAdminId,
                TenantId = systemTenantId,
                SystemRole = SystemRole.SuperAdmin,
                EmployeeNo = "SYS-ADM",
                Email = adminEmail,
                FirstName = "System",
                LastName = "Admin",
                PasswordHash = passwordHash,
                PinHash = numericPinHash, 
                HiredAt = DateOnly.FromDateTime(new DateTime(2024, 1, 1)),
                IsActive = true
            };
            dbContext.Staff.Add(admin);
        }
        else
        {
            admin.Email = adminEmail;
            admin.PasswordHash = passwordHash;
            admin.PinHash = numericPinHash;
            admin.IsActive = true;
            dbContext.Staff.Update(admin);
        }

        dbContext.SaveChanges();
        Log.Information("SuperAdmin seeding completed successfully.");
    }

    public static void SyncCustomerCardPoints(this IApplicationBuilder app)
    {
        using IServiceScope scope = app.ApplicationServices.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<RetailOsDbContext>();

        // Look for customer with email adejorooluwatobi1@gmail.com
        var customer = dbContext.Customers.IgnoreQueryFilters().FirstOrDefault(c => c.Email == "adejorooluwatobi1@gmail.com");
        if (customer != null)
        {
            var existingLedger = dbContext.Set<LoyaltyLedgerEntry>().IgnoreQueryFilters()
                .FirstOrDefault(l => l.CustomerId == customer.Id && l.Delta == 4030);

            if (existingLedger == null)
            {
                customer.PointsBalance += 4030;
                dbContext.Customers.Update(customer);

                dbContext.Set<LoyaltyLedgerEntry>().Add(new LoyaltyLedgerEntry
                {
                    CustomerId = customer.Id,
                    Delta = 4030,
                    Reason = "Card Redemption Credit: CHDN801885955477 (-₦403,000)",
                    BalanceAfter = customer.PointsBalance,
                    CreatedAt = DateTimeOffset.UtcNow
                });

                dbContext.SaveChanges();
                Log.Information("Retroactively credited 4,030 loyalty points to customer {Email}.", customer.Email);
            }
        }
    }
}