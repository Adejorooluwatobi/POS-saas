using POS.Domain.Enums;

namespace POS.Application.DTOs;

public class UpdateStaffDto
{
    public Guid Id { get; set; }
    public Guid? StoreId { get; set; }
    public Guid? RoleId { get; set; }
    public SystemRole SystemRole { get; set; }
    public string Email { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public bool IsActive { get; set; }
    public string? Pin { get; set; }
    public string? Password { get; set; }
}
