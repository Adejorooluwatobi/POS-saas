namespace POS.Application.DTOs;

public class CreateTenantDto
{
    public string Slug { get; set; } = default!;
    public string BusinessName { get; set; } = default!;
    public string ContactEmail { get; set; } = default!;
    public string? ContactPhone { get; set; }
    public string Country { get; set; } = "Nigeria";
    public string? LogoUrl { get; set; }

    // Admin Account
    public string AdminEmail { get; set; } = default!;
    public string AdminPassword { get; set; } = default!;
    public string AdminFirstName { get; set; } = default!;
    public string AdminLastName { get; set; } = default!;

    // Initial Limits
    public int MaxStores { get; set; } = 1;
    public int MaxStaff { get; set; } = 5;
    public int MaxTerminals { get; set; } = 2;

    // Subscription & Trial Duration
    public POS.Domain.Enums.SubscriptionPlan Plan { get; set; } = POS.Domain.Enums.SubscriptionPlan.Starter;
    public string DurationType { get; set; } = "30_DAYS"; // 14_DAYS, 30_DAYS, 60_DAYS, 90_DAYS, 1_YEAR, CUSTOM
    public DateTimeOffset? CustomExpiryDate { get; set; }
    public bool IsTrial { get; set; } = true;
}
