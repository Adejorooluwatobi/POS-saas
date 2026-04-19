using System;
using System.Collections.Generic;
using POS.Domain.Common;

namespace POS.Domain.Entities;

[Auditable]
public class TransactionItem : BaseEntity
{
    public required Guid TransactionId { get; set; }
    public required Guid VariantId { get; set; }
    public required decimal Quantity { get; set; }
    public required decimal UnitPrice { get; set; }
    public required decimal OriginalPrice { get; set; }
    public decimal DiscountAmount { get; set; } = 0m;
    public decimal TaxRate { get; set; } = 0m;
    public decimal TaxAmount { get; set; } = 0m;
    public required decimal LineTotal { get; set; }
    public bool IsVoided { get; set; } = false;
    public DateTimeOffset? VoidedAt { get; set; }

    // Navigation
    public Transaction Transaction { get; set; } = null!;
    public ProductVariant Variant { get; set; } = null!;
    public ICollection<AppliedDiscount> AppliedDiscounts { get; set; } = [];
}
