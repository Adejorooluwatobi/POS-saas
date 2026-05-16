using System;
using System.Collections.Generic;
using POS.Domain.Common;
using POS.Domain.Enums;

namespace POS.Domain.Entities;

[Auditable]
public class Product : AuditableEntity
{
    public required Guid TenantId { get; set; }
    public Guid? StoreId { get; set; }
    public Guid? CategoryId { get; set; }
    public required string MasterSku { get; set; }
    public required string Name { get; set; }
    public string? Brand { get; set; }
    public string? Description { get; set; }
    public TaxCategory TaxCategory { get; set; } = TaxCategory.Standard;
    public decimal TaxRate { get; set; } = 7.5m;
    public bool IsActive { get; set; } = true;

    // Packaging Ratios
    public int? SinglesPerRoll { get; set; }
    public int? RollsPerPack { get; set; }
    public int? SinglesPerPack { get; set; }

    // Bulk Pricing
    public decimal? RollPrice { get; set; }
    public decimal? PackPrice { get; set; }

    // Navigation
    public Tenant Tenant { get; set; } = null!;
    public Category? Category { get; set; }
    public ICollection<ProductVariant> Variants { get; set; } = [];
    public ICollection<StoreProductOverride> StoreOverrides { get; set; } = [];
}
