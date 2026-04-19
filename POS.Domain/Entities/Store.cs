using System;
using System.Collections.Generic;
using POS.Domain.Common;

namespace POS.Domain.Entities;

[Auditable]
public class Store : AuditableEntity
{
    public required Guid TenantId { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
    public required string Address { get; set; }
    public required string City { get; set; }
    public string? State { get; set; }
    public required string Country { get; set; } = "Nigeria";
    public string? Phone { get; set; }
    public required string Timezone { get; set; } = "Africa/Lagos";
    public bool IsActive { get; set; } = true;

    // Navigation
    public Tenant Tenant { get; set; } = null!;
    public ICollection<Terminal> Terminals { get; set; } = [];
    public ICollection<Staff> Staff { get; set; } = [];
    public ICollection<Inventory> Inventories { get; set; } = [];
    public ICollection<Transaction> Transactions { get; set; } = [];
}
