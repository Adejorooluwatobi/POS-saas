using POS.Domain.Enums;

namespace POS.Application.DTOs;

public class AuditLogDto
{
    public Guid Id { get; set; }
    public Guid? TenantId { get; set; }
    public Guid? StoreId { get; set; }
    public Guid? UserId { get; set; }
    public AuditAction Action { get; set; }
    public string EntityType { get; set; } = default!;
    public string EntityId { get; set; } = default!;
    public string? IpAddress { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
