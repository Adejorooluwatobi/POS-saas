using System;
using System.Collections.Generic;
using POS.Domain.Common;

namespace POS.Domain.Entities;

[Auditable]
public class GiftCard : BaseEntity
{
    public required Guid TenantId { get; set; }
    public required string CardNumber { get; set; }
    public string? PinHash { get; set; }
    public required decimal Balance { get; set; }
    public required decimal InitialValue { get; set; }
    public DateOnly? ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset IssuedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation
    public Tenant Tenant { get; set; } = null!;
    public ICollection<Payment> Payments { get; set; } = [];
}
