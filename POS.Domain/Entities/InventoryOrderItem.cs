using System;
using POS.Domain.Common;

namespace POS.Domain.Entities;

public class InventoryOrderItem : BaseEntity
{
    public required Guid InventoryOrderId { get; set; }
    public required Guid VariantId { get; set; }
    public int QuantityOrdered { get; set; }
    public int? QuantityReceived { get; set; }
    public int? QuantityDamaged { get; set; }
    public string? DamageNotes { get; set; }
    public string? DamagePhotoUrl { get; set; }

    // Navigation
    public InventoryOrder Order { get; set; } = null!;
    public ProductVariant Variant { get; set; } = null!;
}
