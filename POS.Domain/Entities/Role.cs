using System;
using System.Collections.Generic;
using System.Text.Json;
using POS.Domain.Common;
using POS.Domain.Enums;

namespace POS.Domain.Entities;

public class Role : BaseEntity
{
    public required Guid TenantId { get; set; }
    public required string Name { get; set; }
    public SystemRole SystemRole { get; set; } = SystemRole.Cashier;
    
    // Stored as JSONB
    public JsonDocument Permissions { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation
    public Tenant Tenant { get; set; } = null!;
    public ICollection<Staff> StaffMembers { get; set; } = [];
}
