using System;
using POS.Domain.Common;

namespace POS.Domain.Entities;

public class StockRequisitionItem : BaseEntity
{
    public required Guid StockRequisitionId { get; set; }
    public required Guid VariantId { get; set; }
    public int QuantityRequested { get; set; }
    public int QuantityFulfilled { get; set; } = 0;

    // Navigation
    public StockRequisition Requisition { get; set; } = null!;
    public ProductVariant Variant { get; set; } = null!;
}
