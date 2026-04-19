using System;
using System.Collections.Generic;
using POS.Domain.Common;

namespace POS.Domain.Entities;

[Auditable]
public class Tenant : AuditableEntity
{
    public required string Slug { get; set; }
    public required string BusinessName { get; set; }
    public required string ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public required string Country { get; set; } = "Nigeria";
    public string? LogoUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsVerified { get; set; } = false;

    // Navigation
    public ICollection<Store> Stores { get; set; } = [];
    public ICollection<Staff> Staff { get; set; } = [];
    public ICollection<Customer> Customers { get; set; } = [];
    public ICollection<Promotion> Promotions { get; set; } = [];
    public ICollection<GiftCard> GiftCards { get; set; } = [];
    public TenantSubscription? Subscription { get; set; }
}
