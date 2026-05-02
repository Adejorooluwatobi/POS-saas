using System;
using POS.Domain.Common;

namespace POS.Domain.Entities;

public class ProductBarcode : BaseEntity
{
    public required Guid TenantId { get; set; }
    public required Guid VariantId { get; set; }
    public Guid? StoreId { get; set; }
    public required string BarcodeValue { get; set; }

    // Navigation
    public Tenant Tenant { get; set; } = null!;
    public ProductVariant Variant { get; set; } = null!;
}
