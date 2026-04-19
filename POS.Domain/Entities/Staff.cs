using System;
using System.Collections.Generic;
using POS.Domain.Common;
using POS.Domain.Enums;

namespace POS.Domain.Entities;

[Auditable]
public class Staff : BaseEntity
{
    public required Guid TenantId { get; set; }
    public Guid? StoreId { get; set; }
    public Guid? RoleId { get; set; }
    public SystemRole SystemRole { get; set; } = SystemRole.Cashier;
    public required string EmployeeNo { get; set; }
    public required string Email { get; set; }
    public required string PinHash { get; set; }
    public string? PasswordHash { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public bool IsActive { get; set; } = true;
    public required DateOnly HiredAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Computed
    public string FullName => $"{FirstName} {LastName}";

    // Navigation
    public Tenant Tenant { get; set; } = null!;
    public Store? Store { get; set; }
    public Role? Role { get; set; }
    public ICollection<TillSession> TillSessions { get; set; } = [];
    public ICollection<Transaction> Transactions { get; set; } = [];
    public ICollection<AuditLog> AuditLogs { get; set; } = [];
}
