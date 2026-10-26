using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using POS.Domain.Entities;
using POS.Domain.Repositories;

namespace POS.Api.Middleware;

public class TenantSubscriptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMemoryCache _cache;

    public TenantSubscriptionMiddleware(RequestDelegate next, IMemoryCache cache)
    {
        _next = next;
        _cache = cache;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";

        // Skip public or bypass endpoints
        if (path.StartsWith("/api/auth/login") ||
            path.StartsWith("/api/terminals/pair") ||
            path.StartsWith("/scalar") ||
            path.StartsWith("/openapi") ||
            path.StartsWith("/swagger") ||
            path.StartsWith("/health"))
        {
            await _next(context);
            return;
        }

        var user = context.User;
        if (user.Identity?.IsAuthenticated == true)
        {
            var role = user.FindFirst("system_role")?.Value;
            
            // SuperAdmins are exempt from tenant subscription restrictions
            if (role != "SuperAdmin")
            {
                var tenantIdClaim = user.FindFirst("tenant_id")?.Value;
                if (!string.IsNullOrEmpty(tenantIdClaim) && Guid.TryParse(tenantIdClaim, out var tenantId))
                {
                    var cacheKey = $"tenant_sub_status_{tenantId}";
                    if (!_cache.TryGetValue(cacheKey, out bool isExpired))
                    {
                        var scope = context.RequestServices.CreateScope();
                        var subRepo = scope.ServiceProvider.GetRequiredService<ITenantSubscriptionRepository>();
                        var sub = await subRepo.GetByTenantAsync(tenantId);

                        isExpired = sub != null && sub.IsExpired;
                        _cache.Set(cacheKey, isExpired, TimeSpan.FromMinutes(2));
                    }

                    if (isExpired)
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                        context.Response.ContentType = "application/json";

                        var payload = new
                        {
                            error = "SubscriptionExpired",
                            code = "SUBSCRIPTION_EXPIRED",
                            message = "Your organization's subscription or free trial has expired. All operations are suspended. Please contact platform administration to renew your access."
                        };

                        await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
                        return;
                    }
                }
            }
        }

        await _next(context);
    }
}
