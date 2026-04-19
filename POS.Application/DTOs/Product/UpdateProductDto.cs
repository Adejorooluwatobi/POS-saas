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
    public bool IsActive { get; set; }
}
