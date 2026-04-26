using POS.Domain.Enums;

namespace POS.Application.DTOs;

public class CreateProductDto
{
    public Guid? CategoryId { get; set; }
    public string MasterSku { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? Brand { get; set; }
    public string? Description { get; set; }
    public TaxCategory TaxCategory { get; set; } = TaxCategory.Standard;
    public decimal? WeightGrams { get; set; }
    public string UnitOfMeasure { get; set; } = "Each";
}
