using System;

namespace POS.Domain.Interfaces;

public interface ITenantContext
{
    Guid? TenantId { get; }
    Guid? UserId { get; }
    Guid? StoreId { get; }
    string? UserName { get; }
    string SystemRole { get; }
    bool IsSuperAdmin => SystemRole == "SuperAdmin";
}
