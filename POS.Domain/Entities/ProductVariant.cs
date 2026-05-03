using System;
using System.Collections.Generic;
using System.Text.Json;
using POS.Domain.Common;

namespace POS.Domain.Entities;

public class ProductVariant : BaseEntity
{
    public required Guid TenantId { get; set; }
    public required Guid ProductId { get; set; }
    public required string Barcode { get; set; }
    public required string Sku { get; set; }

    // Stored as JSONB
    public JsonDocument Attributes { get; set; } = null!;

    public required decimal BasePrice { get; set; }
    public decimal? CostPrice { get; set; }
    public decimal? WeightGrams { get; set; }
    public string UnitOfMeasure { get; set; } = "Each";
    public bool IsActive { get; set; } = true;
    public int LowStockThreshold { get; set; } = 10;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    // UOM Conversion
    public bool IsBaseUnit { get; set; } = true;
    public decimal ConversionFactor { get; set; } = 1m;
    public Guid? BaseVariantId { get; set; }

    // Navigation
    public Tenant Tenant { get; set; } = null!;
    public Product Product { get; set; } = null!;
    public ProductVariant? BaseVariant { get; set; }
    public ICollection<ProductBarcode> Barcodes { get; set; } = [];
    public ICollection<Inventory> Inventories { get; set; } = [];
    public ICollection<PricingRule> PricingRules { get; set; } = [];
    public ICollection<TransactionItem> TransactionItems { get; set; } = [];
    public ICollection<InventoryOrderItem> InventoryOrderItems { get; set; } = [];
    public ICollection<StockRequisitionItem> StockRequisitionItems { get; set; } = [];
}
