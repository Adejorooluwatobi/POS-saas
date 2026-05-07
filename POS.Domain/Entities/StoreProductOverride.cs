using System;
using POS.Domain.Common;

namespace POS.Domain.Entities;

public class StoreProductOverride : AuditableEntity
{
    public required Guid TenantId { get; set; }
    public required Guid ProductId { get; set; }
    public required Guid StoreId { get; set; }
    
    // Overrideable fields
    public decimal? Price { get; set; }
    public decimal? RollPrice { get; set; }
    public decimal? PackPrice { get; set; }
    public bool? IsActive { get; set; }
    public string? ModifiedBy { get; set; }

    // Navigation
    public Tenant Tenant { get; set; } = null!;
    public Product Product { get; set; } = null!;
    public Store Store { get; set; } = null!;
}
