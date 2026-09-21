using System;
using POS.Domain.Common;

namespace POS.Domain.Entities;

[Auditable]
public class InventoryBatch : BaseEntity
{
    public required Guid TenantId { get; set; }
    public required Guid InventoryId { get; set; }
    public required Guid VariantId { get; set; }
    public required Guid StoreId { get; set; }
    public required string BatchNumber { get; set; }
    public DateTimeOffset? ProductionDate { get; set; }
    public DateTimeOffset? ExpiryDate { get; set; }
    public int QuantityOnHand { get; set; } = 0;
    public int QuantityReserved { get; set; } = 0;
    public int ExpiryAlertPercentage { get; set; } = 30;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Computed
    public int QuantityAvailable => QuantityOnHand - QuantityReserved;

    // Navigation
    public Inventory Inventory { get; set; } = null!;
    public ProductVariant Variant { get; set; } = null!;
    public Store Store { get; set; } = null!;
    public Tenant Tenant { get; set; } = null!;
}
