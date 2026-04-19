using System;
using System.Collections.Generic;
using POS.Domain.Common;
using POS.Domain.Enums;

namespace POS.Domain.Entities;

public class Product : AuditableEntity
{
    public required Guid TenantId { get; set; }
    public Guid? CategoryId { get; set; }
    public required string MasterSku { get; set; }
    public required string Name { get; set; }
    public string? Brand { get; set; }
    public string? Description { get; set; }
    public TaxCategory TaxCategory { get; set; } = TaxCategory.Standard;
    public bool IsActive { get; set; } = true;

    // Navigation
    public Tenant Tenant { get; set; } = null!;
    public Category? Category { get; set; }
    public ICollection<ProductVariant> Variants { get; set; } = [];
}
