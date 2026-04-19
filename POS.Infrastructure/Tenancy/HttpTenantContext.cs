using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using POS.Domain.Interfaces;

namespace POS.Infrastructure.Tenancy;

public class HttpTenantContext : ITenantContext
{
    public Guid? TenantId { get; }
    public Guid? UserId { get; }
    public Guid? StoreId { get; }
    public string SystemRole { get; }

    public HttpTenantContext(IHttpContextAccessor accessor)
    {
        var user = accessor.HttpContext?.User;
        if (user == null)
        {
            SystemRole = "Anonymous";
            return;
        }

        var tenantIdClaim = user.FindFirst("tenant_id")?.Value;
        TenantId = tenantIdClaim != null ? Guid.Parse(tenantIdClaim) : null;

        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        UserId = userIdClaim != null ? Guid.Parse(userIdClaim) : null;

        var storeIdClaim = user.FindFirst("store_id")?.Value;
        StoreId = storeIdClaim != null ? Guid.Parse(storeIdClaim) : null;

        SystemRole = user.FindFirst("system_role")?.Value ?? "Cashier";
    }
}
