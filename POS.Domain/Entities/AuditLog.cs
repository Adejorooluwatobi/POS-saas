using System;
using System.Text.Json;
using POS.Domain.Common;
using POS.Domain.Enums;

namespace POS.Domain.Entities;

public class AuditLog : BaseEntity
{
    public Guid? TenantId { get; set; }
    public Guid? StoreId { get; set; }
    public Guid? UserId { get; set; }
    
    public required AuditAction Action { get; set; }
    public required string EntityType { get; set; }
    public required string EntityId { get; set; }
    
    // Enterprise Trail: Old vs New Values stored as JSONB
    public JsonDocument? Changes { get; set; }
    
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public JsonDocument? ActorMetadata { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation
    public Tenant? Tenant { get; set; }
    public Staff? User { get; set; }
}
