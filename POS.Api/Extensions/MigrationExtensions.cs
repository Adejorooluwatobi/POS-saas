using Microsoft.EntityFrameworkCore;
using POS.Infrastructure.Data;

namespace POS.Api.Extensions;

public static class MigrationExtensions
{
    public static void ApplyMigrations(this IApplicationBuilder app)
    {
        using IServiceScope scope = app.ApplicationServices.CreateScope();

        using RetailOsDbContext dbContext = 
            scope.ServiceProvider.GetRequiredService<RetailOsDbContext>();

        dbContext.Database.Migrate();
    }
}
