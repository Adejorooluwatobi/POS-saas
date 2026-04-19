namespace POS.Application.DTOs;

public class CategoryDto
{
    public Guid Id { get; set; }
    public Guid? ParentId { get; set; }
    public string Name { get; set; } = default!;
    public string Slug { get; set; } = default!;
    public short Depth { get; set; }
    public bool IsActive { get; set; }
}
