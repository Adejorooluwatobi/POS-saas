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
    public Guid? TerminalId { get; }
    public string? UserName { get; }
    public string SystemRole { get; }
    public string? IpAddress { get; }
    public string? UserAgent { get; }
    public string? TraceId { get; }
    public string? RequestPath { get; }

    public HttpTenantContext(IHttpContextAccessor accessor)
    {
        var context = accessor.HttpContext;
        if (context == null)
        {
            SystemRole = "Anonymous";
            return;
        }

        var user = context.User;
        
        // Context Info
        IpAddress = context.Connection.RemoteIpAddress?.ToString();
        UserAgent = context.Request.Headers["User-Agent"].ToString();
        TraceId = context.TraceIdentifier;
        RequestPath = context.Request.Path;

        SystemRole = user.FindFirst("system_role")?.Value ?? "Cashier";

        // Identity Info
        var tenantIdClaim = user.FindFirst("tenant_id")?.Value;
        if (!string.IsNullOrEmpty(tenantIdClaim) && Guid.TryParse(tenantIdClaim, out var tid))
        {
            TenantId = tid;
        }
        else if (SystemRole == "SuperAdmin")
        {
            // Allow SuperAdmin to scope context via header or query string
            var headerTenant = context.Request.Headers["X-Tenant-Id"].ToString();
            if (!string.IsNullOrEmpty(headerTenant) && Guid.TryParse(headerTenant, out var parsedTenantId))
            {
                TenantId = parsedTenantId;
            }
            else if (context.Request.Query.TryGetValue("tenantId", out var qTenant) && Guid.TryParse(qTenant.ToString(), out var qTenantId))
            {
                TenantId = qTenantId;
            }
        }

        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        UserId = userIdClaim != null ? Guid.Parse(userIdClaim) : null;

        UserName = user.FindFirst(ClaimTypes.Name)?.Value;

        var storeIdClaim = user.FindFirst("store_id")?.Value;
        if (!string.IsNullOrEmpty(storeIdClaim) && Guid.TryParse(storeIdClaim, out var sid))
        {
            StoreId = sid;
        }
        else if (SystemRole == "SuperAdmin" || SystemRole == "TenantAdmin" || SystemRole == "Manager")
        {
            // Allow SuperAdmin and TenantAdmin to scope store via header or query string
            var headerStore = context.Request.Headers["X-Store-Id"].ToString();
            if (!string.IsNullOrEmpty(headerStore) && Guid.TryParse(headerStore, out var parsedStoreId))
            {
                StoreId = parsedStoreId;
            }
            else if (context.Request.Query.TryGetValue("storeId", out var qStore) && Guid.TryParse(qStore.ToString(), out var qStoreId))
            {
                StoreId = qStoreId;
            }
        }

        var terminalIdClaim = user.FindFirst("terminal_id")?.Value 
            ?? context.Request.Headers["X-Terminal-Id"].ToString();
        if (!string.IsNullOrEmpty(terminalIdClaim) && Guid.TryParse(terminalIdClaim, out var terminalId))
            TerminalId = terminalId;
    }
}
