using System;
using POS.Domain.Enums;

namespace POS.Application.DTOs;

public class CreateStaffDto
{
    public Guid? StoreId { get; set; }
    public Guid? RoleId { get; set; }
    public SystemRole SystemRole { get; set; } = SystemRole.Cashier;
    public string EmployeeNo { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string? Pin { get; set; }
    public string? Password { get; set; }
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public DateOnly HiredAt { get; set; }
}
