namespace POS.Application.DTOs;

public class RoleDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public Dictionary<string, bool> Permissions { get; set; } = [];
}
