using System;
using System.Collections.Generic;
using System.Text.Json;
using POS.Domain.Common;

namespace POS.Domain.Entities;

public class Role : BaseEntity
{
    public required Guid TenantId { get; set; }
    public required string Name { get; set; }
    
    // Stored as JSONB
    public JsonDocument Permissions { get; set; } = null!;
    public string? Description { get; set; }

    // Navigation
    public Tenant Tenant { get; set; } = null!;
    public ICollection<Staff> StaffMembers { get; set; } = [];
}
