using POS.Infrastructure;
using POS.Infrastructure.Data;
using POS.Infrastructure.Data.Interceptors;
using POS.Infrastructure.Tenancy;
using POS.Domain.Interfaces;
using POS.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
        var connectionString = GetConnectionString(builder.Configuration);

        builder.Services.AddDbContext<RetailOsDbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString)
                   .AddInterceptors(sp.GetRequiredService<AuditInterceptor>());
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

    private static string GetConnectionString(IConfiguration configuration)
    {
        var databaseUrl = configuration["DATABASE_URL"];
        if (!string.IsNullOrWhiteSpace(databaseUrl))
        {
            var uri = new Uri(databaseUrl);
            var userInfo = uri.UserInfo.Split(':', 2);
            var username = userInfo.Length > 0 ? userInfo[0] : string.Empty;
            var password = userInfo.Length > 1 ? userInfo[1] : string.Empty;
            var database = uri.AbsolutePath.TrimStart('/');

            return $"Host={uri.Host};Port={uri.Port};Database={database};Username={username};Password={password};Ssl Mode=Require;Trust Server Certificate=true";
        }

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Database connection string is not configured. Set ConnectionStrings__DefaultConnection or DATABASE_URL.");
        }

        return connectionString;
    }
}
