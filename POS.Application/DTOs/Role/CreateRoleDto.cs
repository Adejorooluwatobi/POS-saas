using POS.Domain.Enums;

namespace POS.Application.DTOs;

public class CreateRoleDto
{
    public string Name { get; set; } = default!;
    public SystemRole SystemRole { get; set; }
    public string? Description { get; set; }
    public Dictionary<string, bool> Permissions { get; set; } = [];
}
