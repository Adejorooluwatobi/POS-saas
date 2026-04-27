using POS.Domain.Enums;

namespace POS.Application.DTOs;

public class StaffDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid? StoreId { get; set; }
    public Guid? RoleId { get; set; }
    public SystemRole SystemRole { get; set; }
    public string EmployeeNo { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public bool IsActive { get; set; }
    public DateOnly HiredAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public bool HasPin { get; set; }
    public bool HasPassword { get; set; }
}
