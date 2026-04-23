using POS.Infrastructure;
using POS.Infrastructure.Data;
using POS.Infrastructure.Data.Interceptors;
using POS.Infrastructure.Tenancy;
using POS.Domain.Interfaces;
using POS.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using POS.Api.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Scalar.AspNetCore;

namespace POS.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // ── Configuration & Environment ────────────────────────────────────
        var config = builder.Configuration;
        var rawConnectionString = Environment.GetEnvironmentVariable("DATABASE_URL") 
            ?? config.GetConnectionString("DefaultConnection");

        // Handle case where environment variable is not set and fallback is the literal placeholder
        if (string.IsNullOrEmpty(rawConnectionString) || rawConnectionString.Contains("${DATABASE_URL}"))
        {
            rawConnectionString = null;
        }

        var databaseUrl = ParseDatabaseUrl(rawConnectionString);
        var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") 
            ?? config["Jwt:Secret"];

        if (string.IsNullOrEmpty(jwtSecret) || jwtSecret.Contains("${JWT_SECRET}"))
        {
            jwtSecret = null;
        }

        // Override the configuration values for the rest of the application
        if (!string.IsNullOrEmpty(databaseUrl))
        {
            builder.Configuration["ConnectionStrings:DefaultConnection"] = databaseUrl;
            Console.WriteLine("Database connection string resolved from environment.");
        }
        else
        {
            // Clear the placeholder if it exists to avoid Npgsql parsing errors
            builder.Configuration["ConnectionStrings:DefaultConnection"] = "";
            Console.WriteLine("Warning: Database connection string is missing or invalid.");
        }
        
        if (!string.IsNullOrEmpty(jwtSecret))
        {
            builder.Configuration["Jwt:Secret"] = jwtSecret;
        }

        // ── Core Services ──────────────────────────────────────────────────
        builder.Services.AddControllers();
        builder.Services.AddOpenApi();
        
        // ── Authentication & Authorization ─────────────────────────────────
        var secret = builder.Configuration["Jwt:Secret"] ?? "RetailOS_SuperSecretKey_BecauseThisIsDev_LengthMustBeAtLeast32Chars!";
        var key = Encoding.UTF8.GetBytes(secret);

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };
            });
        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("SuperAdminOnly", policy => policy.RequireClaim("system_role", "SuperAdmin"));
            options.AddPolicy("AdminOnly", policy => policy.RequireClaim("system_role", "SuperAdmin", "TenantAdmin"));
            options.AddPolicy("StaffOnly", policy => policy.RequireClaim("system_role", "SuperAdmin", "TenantAdmin", "StoreManager", "Cashier"));
            options.AddPolicy("ConsumerOnly", policy => policy.RequireClaim("system_role", "Consumer"));
        });

        // ── Repositories ───────────────────────────────────────────────────
        builder.Services.AddRepositories();
        builder.Services.AddApplicationServices();

        // ── Tenancy & Auditing ─────────────────────────────────────────────
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<ITenantContext, HttpTenantContext>();
        builder.Services.AddScoped<AuditInterceptor>();

        // ── Database Context ───────────────────────────────────────────────
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        
        builder.Services.AddDbContext<RetailOsDbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString)
                   .AddInterceptors(sp.GetRequiredService<AuditInterceptor>())
                   .ConfigureWarnings(w =>
                       // This warning is expected: global query filters use a runtime tenant ID
                       // which EF Core flags as a non-deterministic model change. Intentional.
                       w.Ignore(RelationalEventId.PendingModelChangesWarning));
        });

        var app = builder.Build();

        // ── Pipeline ───────────────────────────────────────────────────────
        app.ApplyMigrations();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference(options =>
            {
                options.Title = "RetailOS POS API";
                options.Theme = ScalarTheme.DeepSpace;
                options.DefaultHttpClient = new(ScalarTarget.CSharp, ScalarClient.HttpClient);
            });
        }

        app.UseHttpsRedirection();
        app.UseAuthentication(); // Ensure Authentication is before Authorization
        app.UseAuthorization();
        app.MapControllers().RequireAuthorization();

        app.Run();
    }

    private static string? ParseDatabaseUrl(string? url)
    {
        if (string.IsNullOrEmpty(url)) return null;

        // If it's already a standard connection string, return as is
        bool isUri = url.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) || 
                     url.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase);

        if (!isUri)
            return url;

        try
        {
            var uri = new Uri(url);
            var userInfo = uri.UserInfo.Split(':');
            var username = userInfo[0];
            var password = userInfo.Length > 1 ? userInfo[1] : "";
            var host = uri.Host;
            var port = uri.Port > 0 ? uri.Port : 5432;
            var database = uri.AbsolutePath.TrimStart('/');

            // Render and other cloud providers often require SSL.
            // Integrated Security=false disables GSSAPI/Kerberos negotiation, preventing
            // Npgsql from probing for libgssapi_krb5.so.2 which is absent on Render.
            return $"Host={host};Port={port};Username={username};Password={password};Database={database};SSL Mode=Require;Trust Server Certificate=true;Integrated Security=false";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing DATABASE_URL: {ex.Message}");
            return url; // Fallback to original if parsing fails
        }
    }
}
