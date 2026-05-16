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

        // Identity Info
        var tenantIdClaim = user.FindFirst("tenant_id")?.Value;
        TenantId = tenantIdClaim != null ? Guid.Parse(tenantIdClaim) : null;

        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        UserId = userIdClaim != null ? Guid.Parse(userIdClaim) : null;

        UserName = user.FindFirst(ClaimTypes.Name)?.Value;

        var storeIdClaim = user.FindFirst("store_id")?.Value;
        StoreId = storeIdClaim != null ? Guid.Parse(storeIdClaim) : null;

        var terminalIdClaim = user.FindFirst("terminal_id")?.Value 
            ?? context.Request.Headers["X-Terminal-Id"].ToString();
        if (!string.IsNullOrEmpty(terminalIdClaim) && Guid.TryParse(terminalIdClaim, out var tid))
            TerminalId = tid;

        SystemRole = user.FindFirst("system_role")?.Value ?? "Cashier";
    }
}
