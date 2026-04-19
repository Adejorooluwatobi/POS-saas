// =============================================================================
// RetailOS POS — Multi-Tenant (SaaS) Layer
// Adds: Tenant, TenantSubscription, SystemRole, ITenantContext
// Updates: Store, Staff, Customer, GiftCard, Promotion, AuditLog
// Pattern: Row-level tenancy via EF Core global query filters
// =============================================================================


// ── File: RetailOS.Domain/Enums/SystemRole.cs ─────────────────────────────────

namespace RetailOS.Domain.Enums;

/// <summary>
/// Controls which portal layer a staff member can access.
/// Separate from the per-store operational Role (Cashier/Supervisor/Manager).
/// </summary>
public enum SystemRole
{
    /// <summary>You (Gold). One account. Platform-wide read + disable tenants.</summary>
    SuperAdmin,

    /// <summary>HQ login for the business that bought your SaaS.
    /// Sees all their branches. Cannot see other tenants.</summary>
    TenantAdmin,

    /// <summary>Branch-level manager. Sees only their assigned StoreId.</summary>
    StoreManager,

    /// <summary>POS-only. PIN login on terminal. No admin portal access.</summary>
    Cashier
}

public enum SubscriptionPlan   { Starter, Pro, Enterprise }
public enum SubscriptionStatus { Trial, Active, PastDue, Cancelled, Suspended }
public enum BillingCycle       { Monthly, Annually }


// ── File: RetailOS.Domain/Entities/Tenant.cs ──────────────────────────────────

using RetailOS.Domain.Common;

namespace RetailOS.Domain.Entities;

/// <summary>
/// Represents one business (e.g. "Walmart Nigeria") that purchased your SaaS.
/// One Tenant → many Stores (branches).
/// </summary>
public class Tenant : AuditableEntity
{
    /// <summary>URL-safe identifier. e.g. "walmart-ng", "shoprite-ng"</summary>
    public required string Slug          { get; set; }
    public required string BusinessName  { get; set; }
    public required string ContactEmail  { get; set; }
    public string?         ContactPhone  { get; set; }
    public required string Country       { get; set; } = "Nigeria";
    public string?         LogoUrl       { get; set; }

    /// <summary>You (Super Admin) can flip this to lock out the entire tenant.</summary>
    public bool IsActive   { get; set; } = true;

    /// <summary>Email verified after registration.</summary>
    public bool IsVerified { get; set; } = false;

    // Navigation
    public ICollection<Store>              Stores       { get; set; } = [];
    public ICollection<Staff>             Staff        { get; set; } = [];
    public ICollection<Customer>          Customers    { get; set; } = [];
    public ICollection<Promotion>         Promotions   { get; set; } = [];
    public ICollection<GiftCard>          GiftCards    { get; set; } = [];
    public TenantSubscription?            Subscription { get; set; }
}


// ── File: RetailOS.Domain/Entities/TenantSubscription.cs ──────────────────────

using RetailOS.Domain.Enums;

namespace RetailOS.Domain.Entities;

public class TenantSubscription : BaseEntity
{
    public required Guid               TenantId           { get; set; }
    public SubscriptionPlan            Plan               { get; set; } = SubscriptionPlan.Trial;
    public SubscriptionStatus          Status             { get; set; } = SubscriptionStatus.Trial;

    // Usage limits per plan
    public int MaxStores    { get; set; } = 1;
    public int MaxTerminals { get; set; } = 2;
    public int MaxStaff     { get; set; } = 5;

    public BillingCycle    BillingCycle        { get; set; } = BillingCycle.Monthly;
    public decimal         MonthlyPrice        { get; set; } = 0m;

    public DateTimeOffset? TrialEndsAt         { get; set; }
    public DateTimeOffset  CurrentPeriodStart  { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset  CurrentPeriodEnd    { get; set; } = DateTimeOffset.UtcNow.AddMonths(1);
    public DateTimeOffset? CancelledAt         { get; set; }
    public DateTimeOffset  CreatedAt           { get; set; } = DateTimeOffset.UtcNow;

    // Navigation
    public Tenant Tenant { get; set; } = null!;

    // Computed helper — not stored
    public bool IsExpired => Status is SubscriptionStatus.Cancelled or SubscriptionStatus.Suspended
                          || CurrentPeriodEnd < DateTimeOffset.UtcNow;
}


// ── File: RetailOS.Domain/Entities/Store.cs (UPDATED) ─────────────────────────

// ONLY CHANGE: add TenantId FK + Tenant navigation property
// Add these two lines to the existing Store entity:

/*
    public required Guid TenantId { get; set; }        // ← ADD THIS
    public Tenant Tenant { get; set; } = null!;         // ← ADD THIS (navigation)
*/


// ── File: RetailOS.Domain/Entities/Staff.cs (UPDATED) ────────────────────────

// CHANGES to existing Staff entity:
/*
    public required Guid TenantId  { get; set; }        // ← ADD: tenant this staff belongs to
    public Guid?         StoreId   { get; set; }        // ← CHANGE: was required, now nullable
    public Guid?         RoleId    { get; set; }        // ← CHANGE: was required, now nullable
    public SystemRole    SystemRole { get; set; }       // ← ADD: TenantAdmin | StoreManager | Cashier
    public string?       PasswordHash { get; set; }    // ← CHANGE: was required, now optional (Cashiers use PIN only)

    // TenantAdmin: StoreId = null, RoleId = null, SystemRole = TenantAdmin
    // StoreManager: StoreId = assigned branch, SystemRole = StoreManager
    // Cashier: StoreId = assigned branch, RoleId = cashier role, SystemRole = Cashier

    public Tenant Tenant { get; set; } = null!;         // ← ADD navigation
*/


// ── File: RetailOS.Domain/Entities/Customer.cs (UPDATED) ─────────────────────

// ADD TenantId to Customer:
/*
    public required Guid TenantId { get; set; }         // ← ADD: customers belong to one tenant
    public Tenant Tenant { get; set; } = null!;          // ← ADD navigation
*/


// ── File: RetailOS.Domain/Entities/Promotion.cs (UPDATED) ────────────────────

// ADD TenantId to Promotion:
/*
    public required Guid TenantId { get; set; }         // ← ADD: promotions scoped to tenant
    public Tenant Tenant { get; set; } = null!;          // ← ADD navigation
*/


// ── File: RetailOS.Domain/Entities/GiftCard.cs (UPDATED) ─────────────────────

// ADD TenantId to GiftCard (so gift cards work across branches of same tenant):
/*
    public required Guid TenantId { get; set; }         // ← ADD: gift card valid across tenant branches
    public Tenant Tenant { get; set; } = null!;          // ← ADD navigation
*/


// =============================================================================
// TENANT CONTEXT SERVICE — The Core of Multi-Tenancy
// =============================================================================

// ── File: RetailOS.Infrastructure/Tenancy/ITenantContext.cs ──────────────────

namespace RetailOS.Infrastructure.Tenancy;

/// <summary>
/// Injected into DbContext and services.
/// Tells the system WHICH tenant the current request belongs to.
/// For Super Admin: TenantId = null (sees all / acts globally).
/// </summary>
public interface ITenantContext
{
    Guid?  TenantId   { get; }
    string SystemRole { get; }  // "SuperAdmin" | "TenantAdmin" | "StoreManager" | "Cashier"
    Guid?  StoreId    { get; }  // set for StoreManager and Cashier
    bool   IsSuperAdmin => SystemRole == "SuperAdmin";
    bool   IsTenantAdmin => SystemRole == "TenantAdmin";
}


// ── File: RetailOS.Infrastructure/Tenancy/HttpTenantContext.cs ───────────────

using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace RetailOS.Infrastructure.Tenancy;

/// <summary>
/// Reads tenant context from the JWT claims.
/// JWT must include: tenant_id, system_role, store_id (optional).
/// </summary>
public class HttpTenantContext : ITenantContext
{
    public Guid?  TenantId   { get; }
    public string SystemRole { get; }
    public Guid?  StoreId    { get; }

    public HttpTenantContext(IHttpContextAccessor accessor)
    {
        var user = accessor.HttpContext?.User;
        if (user is null) return;

        var tenantClaim = user.FindFirst("tenant_id")?.Value;
        TenantId = tenantClaim is not null ? Guid.Parse(tenantClaim) : null;

        SystemRole = user.FindFirst("system_role")?.Value ?? "Cashier";

        var storeClaim = user.FindFirst("store_id")?.Value;
        StoreId = storeClaim is not null ? Guid.Parse(storeClaim) : null;
    }
}


// =============================================================================
// UPDATED DbContext — with Global Query Filters
// =============================================================================

// ── File: RetailOS.Infrastructure/Data/RetailOsDbContext.cs (UPDATED) ────────

using Microsoft.EntityFrameworkCore;
using RetailOS.Domain.Entities;
using RetailOS.Infrastructure.Tenancy;

namespace RetailOS.Infrastructure.Data;

public class RetailOsDbContext : DbContext
{
    private readonly ITenantContext _tenant;

    public RetailOsDbContext(DbContextOptions<RetailOsDbContext> options, ITenantContext tenant)
        : base(options)
    {
        _tenant = tenant;
    }

    // ── DbSets (add the two new ones) ──────────────────────────────────────
    public DbSet<Tenant>              Tenants              => Set<Tenant>();
    public DbSet<TenantSubscription>  TenantSubscriptions  => Set<TenantSubscription>();

    // ... all previous DbSets remain unchanged ...
    public DbSet<Store>               Stores               => Set<Store>();
    public DbSet<Terminal>            Terminals            => Set<Terminal>();
    public DbSet<TillSession>         TillSessions         => Set<TillSession>();
    public DbSet<Product>             Products             => Set<Product>();
    public DbSet<ProductVariant>      ProductVariants      => Set<ProductVariant>();
    public DbSet<Inventory>           Inventories          => Set<Inventory>();
    public DbSet<Staff>               Staff                => Set<Staff>();
    public DbSet<Customer>            Customers            => Set<Customer>();
    public DbSet<LoyaltyLedgerEntry>  LoyaltyLedger        => Set<LoyaltyLedgerEntry>();
    public DbSet<Promotion>           Promotions           => Set<Promotion>();
    public DbSet<Coupon>              Coupons              => Set<Coupon>();
    public DbSet<Transaction>         Transactions         => Set<Transaction>();
    public DbSet<TransactionItem>     TransactionItems     => Set<TransactionItem>();
    public DbSet<AppliedDiscount>     AppliedDiscounts     => Set<AppliedDiscount>();
    public DbSet<GiftCard>            GiftCards            => Set<GiftCard>();
    public DbSet<Payment>             Payments             => Set<Payment>();
    public DbSet<AuditLog>            AuditLogs            => Set<AuditLog>();
    public DbSet<EodReport>           EodReports           => Set<EodReport>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RetailOsDbContext).Assembly);

        // ── GLOBAL QUERY FILTERS ─────────────────────────────────────────────
        // These apply to EVERY query automatically.
        // Super Admin: _tenant.TenantId is null → filter is bypassed → sees all.
        // Tenant Admin / Store Manager: _tenant.TenantId is set → row-level isolation.

        var tenantId = _tenant.TenantId;

        // Primary tenant-scoped tables
        modelBuilder.Entity<Store>()
            .HasQueryFilter(s => tenantId == null || s.TenantId == tenantId);

        modelBuilder.Entity<Staff>()
            .HasQueryFilter(s => tenantId == null || s.TenantId == tenantId);

        modelBuilder.Entity<Customer>()
            .HasQueryFilter(c => tenantId == null || c.TenantId == tenantId);

        modelBuilder.Entity<Promotion>()
            .HasQueryFilter(p => tenantId == null || p.TenantId == tenantId);

        modelBuilder.Entity<GiftCard>()
            .HasQueryFilter(g => tenantId == null || g.TenantId == tenantId);

        // Tenant table: non-Super Admins can only see their own tenant record
        modelBuilder.Entity<Tenant>()
            .HasQueryFilter(t => tenantId == null || t.Id == tenantId);

        // ── STORE MANAGER FILTER (applied on top of tenant filter) ─────────
        // If user is a StoreManager, further limit to their specific store.
        // We handle this in the service layer, not query filter, to keep filters simple.
        // StoreManager queries call: .Where(x => x.StoreId == _tenant.StoreId)
    }

    // Auto-update UpdatedAt
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries<AuditableEntity>()
            .Where(e => e.State is EntityState.Added or EntityState.Modified);

        foreach (var entry in entries)
        {
            entry.Entity.UpdatedAt = DateTimeOffset.UtcNow;
            if (entry.State == EntityState.Added)
                entry.Entity.CreatedAt = DateTimeOffset.UtcNow;
        }
    }
}


// =============================================================================
// NEW: ENTITY TYPE CONFIGURATIONS for Tenant tables
// =============================================================================

// ── File: Configurations/TenantConfiguration.cs ──────────────────────────────

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetailOS.Domain.Entities;

namespace RetailOS.Infrastructure.Data.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(t => t.Slug).IsRequired().HasMaxLength(80);
        builder.Property(t => t.BusinessName).IsRequired().HasMaxLength(200);
        builder.Property(t => t.ContactEmail).IsRequired().HasMaxLength(200);
        builder.Property(t => t.ContactPhone).HasMaxLength(30);
        builder.Property(t => t.Country).IsRequired().HasMaxLength(100).HasDefaultValue("Nigeria");
        builder.Property(t => t.LogoUrl).HasMaxLength(500);

        builder.HasIndex(t => t.Slug).IsUnique();
        builder.HasIndex(t => t.ContactEmail).IsUnique();
        builder.HasIndex(t => t.IsActive);

        // 1:1 with TenantSubscription
        builder.HasOne(t => t.Subscription)
               .WithOne(s => s.Tenant)
               .HasForeignKey<TenantSubscription>(s => s.TenantId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class TenantSubscriptionConfiguration : IEntityTypeConfiguration<TenantSubscription>
{
    public void Configure(EntityTypeBuilder<TenantSubscription> builder)
    {
        builder.ToTable("TenantSubscriptions");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(s => s.Plan).HasConversion<string>().IsRequired();
        builder.Property(s => s.Status).HasConversion<string>().IsRequired();
        builder.Property(s => s.BillingCycle).HasConversion<string>().IsRequired();
        builder.Property(s => s.MonthlyPrice).HasPrecision(10, 2);

        builder.Ignore(s => s.IsExpired);  // computed, not stored

        builder.HasIndex(s => s.TenantId).IsUnique();
        builder.HasIndex(s => s.Status);
        builder.HasIndex(s => s.CurrentPeriodEnd);  // for expired subscription jobs
    }
}


// ── Updated StoreConfiguration.cs — add TenantId FK ─────────────────────────

public class StoreConfiguration : IEntityTypeConfiguration<Store>
{
    public void Configure(EntityTypeBuilder<Store> builder)
    {
        builder.ToTable("Stores");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(s => s.Code).IsRequired().HasMaxLength(10);
        builder.Property(s => s.Name).IsRequired().HasMaxLength(200);
        builder.Property(s => s.Timezone).IsRequired().HasMaxLength(60).HasDefaultValue("Africa/Lagos");

        builder.HasIndex(s => s.Code).IsUnique();
        builder.HasIndex(s => s.TenantId);           // ← NEW: query stores by tenant
        builder.HasIndex(s => new { s.TenantId, s.IsActive });  // ← NEW: active stores per tenant

        // ← NEW: Store → Tenant relationship
        builder.HasOne(s => s.Tenant)
               .WithMany(t => t.Stores)
               .HasForeignKey(s => s.TenantId)
               .OnDelete(DeleteBehavior.Restrict);  // disabling tenant doesn't delete stores
    }
}


// ── Updated StaffConfiguration.cs — add TenantId FK + SystemRole ─────────────

public class StaffConfiguration : IEntityTypeConfiguration<Staff>
{
    public void Configure(EntityTypeBuilder<Staff> builder)
    {
        builder.ToTable("Staff");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(s => s.EmployeeNo).IsRequired().HasMaxLength(30);
        builder.Property(s => s.Email).IsRequired().HasMaxLength(200);
        builder.Property(s => s.PinHash).IsRequired().HasMaxLength(255);
        builder.Property(s => s.PasswordHash).HasMaxLength(255);  // nullable now
        builder.Property(s => s.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(s => s.LastName).IsRequired().HasMaxLength(100);
        builder.Property(s => s.SystemRole).HasConversion<string>().IsRequired();  // ← NEW

        builder.Ignore(s => s.FullName);

        builder.HasIndex(s => s.EmployeeNo).IsUnique();
        builder.HasIndex(s => s.Email).IsUnique();
        builder.HasIndex(s => s.TenantId);           // ← NEW
        builder.HasIndex(s => new { s.TenantId, s.SystemRole });  // ← NEW: query admins by tenant

        // ← NEW: Staff → Tenant
        builder.HasOne(s => s.Tenant)
               .WithMany(t => t.Staff)
               .HasForeignKey(s => s.TenantId)
               .OnDelete(DeleteBehavior.Cascade);

        // StoreId is now optional (TenantAdmin has no store)
        builder.HasOne(s => s.Store)
               .WithMany(st => st.Staff)
               .HasForeignKey(s => s.StoreId)
               .IsRequired(false)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Role)
               .WithMany(r => r.StaffMembers)
               .HasForeignKey(s => s.RoleId)
               .IsRequired(false)
               .OnDelete(DeleteBehavior.Restrict);
    }
}


// =============================================================================
// JWT TOKEN GENERATION — What Claims to Include
// =============================================================================

// ── File: RetailOS.API/Services/TokenService.cs ───────────────────────────────

/*
public class TokenService
{
    public string GenerateToken(Staff staff)
    {
        var claims = new List<Claim>
        {
            new("sub",          staff.Id.ToString()),
            new("email",        staff.Email),
            new("tenant_id",    staff.TenantId.ToString()),
            new("system_role",  staff.SystemRole.ToString()),
            // Only include store_id for StoreManager and Cashier
            new("store_id",     staff.StoreId?.ToString() ?? ""),
            new("name",         staff.FullName),
        };

        // Sign and return JWT...
    }
}

// Super Admin JWT (you, Gold):
// {
//   "sub":         "super-admin-guid",
//   "tenant_id":   null,           ← no tenant = bypass all filters = see everything
//   "system_role": "SuperAdmin",
//   "store_id":    null
// }

// Tenant Admin JWT (Walmart Nigeria HQ):
// {
//   "sub":         "walmart-admin-guid",
//   "tenant_id":   "walmart-tenant-guid",   ← scoped to their tenant only
//   "system_role": "TenantAdmin",
//   "store_id":    null                      ← sees all branches
// }

// Store Manager JWT:
// {
//   "sub":         "temi-guid",
//   "tenant_id":   "walmart-tenant-guid",
//   "system_role": "StoreManager",
//   "store_id":    "vi-store-guid"           ← scoped to one branch
// }

// Cashier JWT (POS PIN login):
// {
//   "sub":         "gold-cashier-guid",
//   "tenant_id":   "walmart-tenant-guid",
//   "system_role": "Cashier",
//   "store_id":    "vi-store-guid"
// }
*/


// =============================================================================
// REGISTRATION SERVICE — How a Tenant Signs Up
// =============================================================================

// ── File: RetailOS.Application/Services/TenantRegistrationService.cs ──────────

/*
public class TenantRegistrationService
{
    private readonly RetailOsDbContext _db;

    /// <summary>
    /// Called when a new business signs up on your platform.
    /// Creates: Tenant + TenantSubscription + first TenantAdmin staff account.
    /// </summary>
    public async Task<Tenant> RegisterAsync(RegisterTenantRequest request)
    {
        // 1. Validate slug uniqueness
        if (await _db.Tenants.AnyAsync(t => t.Slug == request.Slug))
            throw new DomainException("Business slug already taken");

        // 2. Create the Tenant
        var tenant = new Tenant
        {
            Slug         = request.Slug,           // e.g. "walmart-ng"
            BusinessName = request.BusinessName,   // e.g. "Walmart Nigeria"
            ContactEmail = request.Email,
            ContactPhone = request.Phone,
            Country      = request.Country,
        };

        // 3. Create a trial subscription (14 days, Starter plan)
        var subscription = new TenantSubscription
        {
            TenantId           = tenant.Id,
            Plan               = SubscriptionPlan.Starter,
            Status             = SubscriptionStatus.Trial,
            MaxStores          = 1,
            MaxTerminals       = 2,
            MaxStaff           = 5,
            TrialEndsAt        = DateTimeOffset.UtcNow.AddDays(14),
            CurrentPeriodStart = DateTimeOffset.UtcNow,
            CurrentPeriodEnd   = DateTimeOffset.UtcNow.AddDays(14),
        };
        tenant.Subscription = subscription;

        // 4. Create the first TenantAdmin account (their HQ login)
        var adminStaff = new Staff
        {
            TenantId     = tenant.Id,
            StoreId      = null,        // TenantAdmin has no store
            RoleId       = null,
            SystemRole   = SystemRole.TenantAdmin,
            EmployeeNo   = $"{request.Slug.ToUpper()}-ADMIN",
            Email        = request.Email,
            PinHash      = "",          // TenantAdmin uses password, not PIN
            PasswordHash = BCrypt.HashPassword(request.Password),
            FirstName    = request.FirstName,
            LastName     = request.LastName,
            HiredAt      = DateOnly.FromDateTime(DateTime.Today),
        };

        _db.Tenants.Add(tenant);
        _db.Staff.Add(adminStaff);
        await _db.SaveChangesAsync();

        // 5. Send welcome + verification email
        // await _emailService.SendWelcomeAsync(tenant, adminStaff);

        return tenant;
    }
}

// RegisterTenantRequest DTO:
public record RegisterTenantRequest(
    string Slug,
    string BusinessName,
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string Phone,
    string Country
);
*/


// =============================================================================
// STORE REGISTRATION — Tenant Admin adds a branch
// =============================================================================

/*
public class StoreService
{
    private readonly RetailOsDbContext _db;
    private readonly ITenantContext _tenant;

    /// <summary>
    /// TenantAdmin calls this to register a new branch.
    /// Validates against subscription limits.
    /// </summary>
    public async Task<Store> CreateStoreAsync(CreateStoreRequest request)
    {
        // 1. Check subscription limits
        var sub = await _db.TenantSubscriptions
            .FirstAsync(s => s.TenantId == _tenant.TenantId);

        var currentStoreCount = await _db.Stores.CountAsync();

        if (currentStoreCount >= sub.MaxStores)
            throw new DomainException($"Your {sub.Plan} plan allows max {sub.MaxStores} stores. Upgrade to add more.");

        // 2. Create the store
        var store = new Store
        {
            TenantId = _tenant.TenantId!.Value,   // must be set; TenantAdmin is authenticated
            Code     = request.Code,
            Name     = request.Name,
            Address  = request.Address,
            City     = request.City,
            Country  = request.Country,
            Timezone = request.Timezone,
        };

        _db.Stores.Add(store);
        await _db.SaveChangesAsync();
        return store;
    }
}
*/


// =============================================================================
// SUPER ADMIN DASHBOARD SERVICE — What you (Gold) can see and do
// =============================================================================

/*
public class SuperAdminService
{
    private readonly RetailOsDbContext _db;

    // NOTE: _db is instantiated with ITenantContext where TenantId = null
    // So all query filters return ALL data across ALL tenants.

    public async Task<PlatformMetricsDto> GetPlatformMetricsAsync()
    {
        return new PlatformMetricsDto
        {
            TotalTenants   = await _db.Tenants.CountAsync(),
            ActiveTenants  = await _db.Tenants.CountAsync(t => t.IsActive),
            TotalStores    = await _db.Stores.CountAsync(),
            TotalTransactions = await _db.Transactions.CountAsync(),
            TodayRevenue   = await _db.Transactions
                .Where(t => t.Status == TransactionStatus.Completed
                         && t.CompletedAt >= DateTimeOffset.UtcNow.Date)
                .SumAsync(t => t.GrandTotal),
        };
    }

    /// <summary>
    /// You (Super Admin) can ONLY disable/enable a tenant.
    /// You cannot touch their stores, staff, products, or settings.
    /// </summary>
    public async Task SetTenantActiveStatusAsync(Guid tenantId, bool isActive, string reason)
    {
        var tenant = await _db.Tenants
            .IgnoreQueryFilters()   // bypass filter since TenantId doesn't match
            .FirstAsync(t => t.Id == tenantId);

        tenant.IsActive = isActive;

        // Log the action to audit trail
        _db.AuditLogs.Add(new AuditLog
        {
            Action     = isActive ? "ENABLE_TENANT" : "DISABLE_TENANT",
            EntityType = "Tenant",
            EntityId   = tenantId,
            AfterData  = JsonDocument.Parse($"{{\"isActive\":{isActive.ToString().ToLower()},\"reason\":\"{reason}\"}}"),
        });

        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// Get all tenants with their subscription and store count.
    /// Only you can call this endpoint.
    /// </summary>
    public async Task<List<TenantSummaryDto>> GetAllTenantsAsync()
    {
        return await _db.Tenants
            .Include(t => t.Subscription)
            .Include(t => t.Stores)
            .IgnoreQueryFilters()  // explicit — we want ALL tenants
            .Select(t => new TenantSummaryDto(
                t.Id, t.BusinessName, t.Slug,
                t.IsActive, t.IsVerified,
                t.Subscription!.Plan.ToString(),
                t.Subscription.Status.ToString(),
                t.Stores.Count,
                t.CreatedAt
            ))
            .ToListAsync();
    }
}
*/


// =============================================================================
// PROGRAM.CS — SERVICE REGISTRATION (updated section)
// =============================================================================

/*
// In Program.cs, add:

// Tenant Context — reads TenantId from JWT, injected into DbContext
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantContext, HttpTenantContext>();

// DbContext — receives ITenantContext for query filters
builder.Services.AddDbContext<RetailOsDbContext>((provider, options) =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsql => {
            npgsql.MigrationsAssembly("RetailOS.Infrastructure");
            npgsql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
        });
});

// Application services
builder.Services.AddScoped<TenantRegistrationService>();
builder.Services.AddScoped<StoreService>();
builder.Services.AddScoped<SuperAdminService>();
// ... other services
*/


// =============================================================================
// MIGRATION COMMANDS (add after existing initial migration)
// =============================================================================

/*
# Add multi-tenant migration
dotnet ef migrations add AddMultiTenancy \
  --project RetailOS.Infrastructure \
  --startup-project RetailOS.API \
  --output-dir Data/Migrations

# What this migration generates:
# - CREATE TABLE "Tenants" (...)
# - CREATE TABLE "TenantSubscriptions" (...)
# - ALTER TABLE "Stores" ADD COLUMN "TenantId" uuid NOT NULL
# - ALTER TABLE "Staff" ADD COLUMN "TenantId" uuid NOT NULL
# - ALTER TABLE "Staff" ADD COLUMN "SystemRole" varchar NOT NULL DEFAULT 'Cashier'
# - ALTER TABLE "Staff" ALTER COLUMN "StoreId" DROP NOT NULL
# - ALTER TABLE "Customers" ADD COLUMN "TenantId" uuid NOT NULL
# - ALTER TABLE "Promotions" ADD COLUMN "TenantId" uuid NOT NULL
# - ALTER TABLE "GiftCards" ADD COLUMN "TenantId" uuid NOT NULL
# - (all FK constraints and indexes)

dotnet ef database update \
  --project RetailOS.Infrastructure \
  --startup-project RetailOS.API
*/
