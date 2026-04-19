using System;
using POS.Domain.Common;

namespace POS.Domain.Entities;

[Auditable]
public class Inventory : BaseEntity
{
    public required Guid VariantId { get; set; }
    public required Guid StoreId { get; set; }
    public int QuantityOnHand { get; set; } = 0;
    public int QuantityReserved { get; set; } = 0;
    public int ReorderPoint { get; set; } = 0;
    public int ReorderQty { get; set; } = 0;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Computed
    public int QuantityAvailable => QuantityOnHand - QuantityReserved;

    // Navigation
    public ProductVariant Variant { get; set; } = null!;
    public Store Store { get; set; } = null!;
}
