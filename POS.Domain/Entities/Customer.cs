using System;
using System.Collections.Generic;
using POS.Domain.Common;
using POS.Domain.Enums;

namespace POS.Domain.Entities;

[Auditable]
public class Customer : AuditableEntity
{
    public required Guid TenantId { get; set; }
    public string? LoyaltyCardNo { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PasswordHash { get; set; }
    public int PointsBalance { get; set; } = 0;
    public CustomerTier Tier { get; set; } = CustomerTier.Bronze;
    public bool IsActive { get; set; } = true;

    // Navigation
    public Tenant Tenant { get; set; } = null!;
    public ICollection<Transaction> Transactions { get; set; } = [];
    public ICollection<LoyaltyLedgerEntry> LoyaltyLedger { get; set; } = [];
}
