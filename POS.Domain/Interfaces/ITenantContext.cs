using System;

namespace POS.Domain.Interfaces;

public interface ITenantContext
{
    Guid? TenantId { get; }
    Guid? UserId { get; }
    Guid? StoreId { get; }
    Guid? TerminalId { get; }
    string? UserName { get; }
    string SystemRole { get; }
    string? IpAddress { get; }
    string? UserAgent { get; }
    string? TraceId { get; }
    string? RequestPath { get; }
    bool IsSuperAdmin => SystemRole == "SuperAdmin";
}
