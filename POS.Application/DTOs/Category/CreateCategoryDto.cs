namespace POS.Application.DTOs;

public class CreateCategoryDto
{
    public Guid? TenantId { get; set; }
    public Guid? ParentId { get; set; }
    public string Name { get; set; } = default!;
    public string Slug { get; set; } = default!;
}
