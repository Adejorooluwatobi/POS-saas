using Microsoft.EntityFrameworkCore;
using POS.Infrastructure.Data;
using POS.Domain.Entities;
using POS.Domain.Enums;
using POS.Domain.Interfaces;

namespace POS.Api.Extensions;

public static class MigrationExtensions
{
    public static void ApplyMigrations(this IApplicationBuilder app)
    {
        using IServiceScope scope = app.ApplicationServices.CreateScope();

        using RetailOsDbContext dbContext = 
            scope.ServiceProvider.GetRequiredService<RetailOsDbContext>();

        // Set timeout to 60 seconds to allow for free-tier latency
        dbContext.Database.SetCommandTimeout(120);
        
        dbContext.Database.Migrate();
    }

    public static void SeedSuperAdmin(this IApplicationBuilder app)
    {
        using IServiceScope scope = app.ApplicationServices.CreateScope();
        var services = scope.ServiceProvider;
        
        var dbContext = services.GetRequiredService<RetailOsDbContext>();
        var config = services.GetRequiredService<IConfiguration>();
        var passwordService = services.GetRequiredService<IPasswordService>();

        // Set timeout for seeding operations as well
        try {
        dbContext.Database.SetCommandTimeout(120);
        dbContext.Database.Migrate();
    }
    catch (Exception ex) {
        // This will print the actual database error to your Render logs
        Console.WriteLine($"MIGRATION ERROR: {ex.Message}");
        Console.WriteLine($"INNER EXCEPTION: {ex.InnerException?.Message}");
        throw; // Still let it crash so you can see it in the logs
    }

        var adminEmail = config["SuperAdmin:Email"];
        var adminPassword = config["SuperAdmin:Password"];

        if (string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminPassword))
        {
            return;
        }

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
    }
}