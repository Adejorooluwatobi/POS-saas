using System;
using POS.Domain.Common;

namespace POS.Domain.Entities;

[Auditable]
public class PricingRule : AuditableEntity
{
    public required Guid TenantId { get; set; }
    public required Guid VariantId { get; set; }
    public Guid? StoreId { get; set; }
    public required decimal SalePrice { get; set; }
    public required DateTimeOffset StartsAt { get; set; }
    public required DateTimeOffset EndsAt { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation
    public Tenant Tenant { get; set; } = null!;
    public ProductVariant Variant { get; set; } = null!;
}
