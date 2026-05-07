using POS.Domain.Enums;

namespace POS.Application.DTOs;

public class ProductDto
{
    public Guid Id { get; set; }
    public Guid? CategoryId { get; set; }
    public string MasterSku { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? Brand { get; set; }
    public string? Description { get; set; }
    public TaxCategory TaxCategory { get; set; }
    public decimal TaxRate { get; set; }
    public bool IsActive { get; set; }
    public int? SinglesPerRoll { get; set; }
    public int? RollsPerPack { get; set; }
    public int? SinglesPerPack { get; set; }
    public decimal? RollPrice { get; set; }
    public decimal? PackPrice { get; set; }
    public decimal BasePrice { get; set; }
    public decimal CostPrice { get; set; }
    public decimal? WeightGrams { get; set; }
    public string? UnitOfMeasure { get; set; } = "Each";
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public List<string> Barcodes { get; set; } = [];
    public Guid? StoreId { get; set; }
    public List<StoreProductOverrideDto> StoreOverrides { get; set; } = [];
    public List<ProductVariantDto> Variants { get; set; } = [];
}

public class ProductVariantDto
{
    public Guid Id { get; set; }
    public string SKU { get; set; } = default!;
    public string Barcode { get; set; } = default!;
    public decimal BasePrice { get; set; }
    public decimal? CostPrice { get; set; }
    public string UnitOfMeasure { get; set; } = "Each";
}

public class StoreProductOverrideDto
{
    public Guid Id { get; set; }
    public Guid StoreId { get; set; }
    public decimal Price { get; set; }
    public decimal? RollPrice { get; set; }
    public decimal? PackPrice { get; set; }
    public bool IsActive { get; set; }
    public string? ModifiedBy { get; set; }
}
