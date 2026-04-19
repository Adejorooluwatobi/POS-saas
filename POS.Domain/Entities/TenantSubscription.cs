using System;
using POS.Domain.Common;
using POS.Domain.Enums;

namespace POS.Domain.Entities;

[Auditable]
public class TenantSubscription : BaseEntity
{
    public required Guid TenantId { get; set; }
    public SubscriptionPlan Plan { get; set; } = SubscriptionPlan.Starter;
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Trial;

    public int MaxStores { get; set; } = 1;
    public int MaxTerminals { get; set; } = 2;
    public int MaxStaff { get; set; } = 5;

    public BillingCycle BillingCycle { get; set; } = BillingCycle.Monthly;
    public decimal MonthlyPrice { get; set; } = 0m;

    public DateTimeOffset? TrialEndsAt { get; set; }
    public DateTimeOffset CurrentPeriodStart { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset CurrentPeriodEnd { get; set; } = DateTimeOffset.UtcNow.AddMonths(1);
    public DateTimeOffset? CancelledAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation
    public Tenant Tenant { get; set; } = null!;

    public bool IsExpired => Status is SubscriptionStatus.Cancelled or SubscriptionStatus.Suspended 
                          || CurrentPeriodEnd < DateTimeOffset.UtcNow;
}
