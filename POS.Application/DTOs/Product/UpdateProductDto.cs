using POS.Domain.Enums;

namespace POS.Application.DTOs;

public class UpdateProductDto
{
    public Guid Id { get; set; }
    public Guid? CategoryId { get; set; }
    public string Name { get; set; } = default!;
    public string? Brand { get; set; }
    public string? Description { get; set; }
    public TaxCategory TaxCategory { get; set; }
    public decimal TaxRate { get; set; }
    public decimal CostPrice { get; set; }
    public decimal SellingPrice { get; set; }
    public decimal? WeightGrams { get; set; }
    public string? UnitOfMeasure { get; set; }
    public bool IsActive { get; set; }
    public int? SinglesPerRoll { get; set; }
    public int? RollsPerPack { get; set; }
    
    // Advanced Store Scoping & Multi-Barcoding
    public List<Guid>? TargetStoreIds { get; set; }
    public List<string>? Barcodes { get; set; }
}
