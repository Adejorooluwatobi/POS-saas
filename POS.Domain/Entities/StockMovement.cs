using System;
using POS.Domain.Common;

namespace POS.Domain.Entities;

public class StockMovement : BaseEntity
{
    public required Guid TenantId { get; set; }
    public required Guid StoreId { get; set; }
    public required Guid VariantId { get; set; }
    public required int QuantityChange { get; set; }
    public required int BalanceAfter { get; set; }
    public string? Reason { get; set; }
    public Guid? ReferenceId { get; set; }
    public string? ReferenceType { get; set; } // e.g., "InventoryOrder", "Transaction", "Manual"
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation
    public Store Store { get; set; } = null!;
    public ProductVariant Variant { get; set; } = null!;
}
