namespace POS.Application.DTOs.Statistics;

public class RevenueStatsDto
{
    public decimal Daily { get; set; }
    public decimal Weekly { get; set; }
    public decimal Monthly { get; set; }
    public decimal Yearly { get; set; }
    public decimal Lifetime { get; set; }
    
    // For specific month/year filtering
    public decimal FilteredRevenue { get; set; }
}

public class StoreDetailsDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string City { get; set; } = null!;
    public bool IsActive { get; set; }
    public RevenueStatsDto Revenue { get; set; } = new();
    public List<StaffStatsDto> Staff { get; set; } = [];
}

public class TenantDetailsDto
{
    public Guid Id { get; set; }
    public string BusinessName { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string ContactEmail { get; set; } = null!;
    public bool IsActive { get; set; }
    public RevenueStatsDto Revenue { get; set; } = new();
    public List<StoreSummaryDto> Stores { get; set; } = [];
    public TenantSubscriptionSummaryDto? Subscription { get; set; }
}

public class StaffStatsDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = null!;
    public string EmployeeNo { get; set; } = null!;
    public string SystemRole { get; set; } = null!;
    public bool IsActive { get; set; }
    public decimal TodayRevenue { get; set; }
    public decimal LifetimeRevenue { get; set; }
}

public class StoreSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
    public bool IsActive { get; set; }
    public decimal MonthlyRevenue { get; set; }
    public decimal LifetimeRevenue { get; set; }
}

public class TenantSubscriptionSummaryDto
{
    public Guid Id { get; set; }
    public int Plan { get; set; }
    public int Status { get; set; }
    public int BillingCycle { get; set; }
    public int MaxStores { get; set; }
    public int MaxStaff { get; set; }
    public int MaxTerminals { get; set; }
    public decimal MonthlyPrice { get; set; }
    public DateTimeOffset CurrentPeriodStart { get; set; }
    public DateTimeOffset CurrentPeriodEnd { get; set; }
    public bool IsActive => Status == 1; // 1 = Active
}
