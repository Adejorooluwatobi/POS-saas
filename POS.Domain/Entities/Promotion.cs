using System;
using System.Collections.Generic;
using System.Text.Json;
using POS.Domain.Common;
using POS.Domain.Enums;

namespace POS.Domain.Entities;

[Auditable]
public class Promotion : BaseEntity
{
    public required Guid TenantId { get; set; }
    public required string Name { get; set; }
    public required PromotionType Type { get; set; }
    public required PromotionScope Scope { get; set; }
    public required decimal Value { get; set; }
    public decimal? MinPurchase { get; set; }
    public decimal? MaxDiscount { get; set; }
    public int? MaxUses { get; set; }
    public int UsedCount { get; set; } = 0;

    // Stored as JSONB
    public JsonDocument? Conditions { get; set; }

    public required DateTimeOffset StartsAt { get; set; }
    public DateTimeOffset? EndsAt { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation
    public Tenant Tenant { get; set; } = null!;
    public ICollection<Coupon> Coupons { get; set; } = [];
    public ICollection<AppliedDiscount> AppliedDiscounts { get; set; } = [];
}
