using System;
using POS.Domain.Enums;

namespace POS.Application.DTOs;

public class StaffDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid? StoreId { get; set; }
    public string? StoreName { get; set; }
    public Guid? RoleId { get; set; }
    public string? RoleName { get; set; }
    public SystemRole SystemRole { get; set; }
    public string EmployeeNo { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public bool MustChangePassword { get; set; }
    public bool MustChangePin { get; set; }
    public string? GeneratedPassword { get; set; }
    public string? GeneratedPin { get; set; }
    public DateOnly HiredAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public bool HasPin { get; set; }
    public bool HasPassword { get; set; }
    public decimal TodayRevenue { get; set; }
    public decimal LifetimeRevenue { get; set; }
}
