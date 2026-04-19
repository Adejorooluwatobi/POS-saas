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
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation
    public Tenant Tenant { get; set; } = null!;
    public Product Product { get; set; } = null!;
    public ICollection<Inventory> Inventories { get; set; } = [];
    public ICollection<PricingRule> PricingRules { get; set; } = [];
    public ICollection<TransactionItem> TransactionItems { get; set; } = [];
}
