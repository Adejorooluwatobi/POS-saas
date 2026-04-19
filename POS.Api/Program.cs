using POS.Infrastructure;
using POS.Infrastructure.Data;
using POS.Infrastructure.Data.Interceptors;
using POS.Infrastructure.Tenancy;
using POS.Domain.Interfaces;
using POS.Application;
using Microsoft.EntityFrameworkCore;
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
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        
        builder.Services.AddDbContext<RetailOsDbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString)
                   .AddInterceptors(sp.GetRequiredService<AuditInterceptor>());
        });

        var app = builder.Build();

        // ── Pipeline ───────────────────────────────────────────────────────
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
}
