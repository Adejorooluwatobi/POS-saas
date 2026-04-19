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
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
