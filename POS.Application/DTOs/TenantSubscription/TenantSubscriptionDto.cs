using POS.Domain.Enums;

namespace POS.Application.DTOs;

public class TenantSubscriptionDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public SubscriptionPlan Plan { get; set; }
    public SubscriptionStatus Status { get; set; }
    public int MaxStores { get; set; }
    public int MaxTerminals { get; set; }
    public int MaxStaff { get; set; }
    public BillingCycle BillingCycle { get; set; }
    public decimal MonthlyPrice { get; set; }
    public DateTimeOffset? TrialEndsAt { get; set; }
    public DateTimeOffset CurrentPeriodStart { get; set; }
    public DateTimeOffset CurrentPeriodEnd { get; set; }
    public DateTimeOffset? CancelledAt { get; set; }
    public bool IsExpired { get; set; }
}

public class UpdateSubscriptionDto
{
    public SubscriptionPlan Plan { get; set; }
    public BillingCycle BillingCycle { get; set; }
}
