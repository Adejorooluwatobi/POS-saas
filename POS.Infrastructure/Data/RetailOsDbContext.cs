using Microsoft.EntityFrameworkCore;
using POS.Domain.Interfaces;
using Microsoft.EntityFrameworkCore.Diagnostics;
using POS.Domain.Entities;
using POS.Domain.Enums;
using POS.Domain.Common;
using POS.Infrastructure.Data.Configurations;
using POS.Infrastructure.Data.Interceptors;
using POS.Infrastructure.Tenancy;

namespace POS.Infrastructure.Data;

public class RetailOsDbContext : DbContext
{
    private readonly ITenantContext _tenant;

    public RetailOsDbContext(DbContextOptions<RetailOsDbContext> options, ITenantContext tenant)
        : base(options)
    {
        _tenant = tenant;
    }

    // ── Platform / SaaS DbSets ─────────────────────────────────────────────
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<TenantSubscription> TenantSubscriptions => Set<TenantSubscription>();

    // ── Domain DbSets ──────────────────────────────────────────────────────
    public DbSet<Store> Stores => Set<Store>();
    public DbSet<Terminal> Terminals => Set<Terminal>();
    public DbSet<TillSession> TillSessions => Set<TillSession>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<Inventory> Inventories => Set<Inventory>();
    public DbSet<PricingRule> PricingRules => Set<PricingRule>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Staff> Staff => Set<Staff>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<LoyaltyLedgerEntry> LoyaltyLedger => Set<LoyaltyLedgerEntry>();
    public DbSet<Promotion> Promotions => Set<Promotion>();
    public DbSet<Coupon> Coupons => Set<Coupon>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<TransactionItem> TransactionItems => Set<TransactionItem>();
    public DbSet<AppliedDiscount> AppliedDiscounts => Set<AppliedDiscount>();
    public DbSet<GiftCard> GiftCards => Set<GiftCard>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<EodReport> EodReports => Set<EodReport>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all IEntityTypeConfiguration<T> in this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RetailOsDbContext).Assembly);

        // ── GLOBAL QUERY FILTERS (Multi-Tenancy) ─────────────────────────────
        var tenantId = _tenant.TenantId;

        // Tenant can only see their own record (SuperAdmin sees all)
        modelBuilder.Entity<Tenant>().HasQueryFilter(t => tenantId == null || t.Id == tenantId);

        // Core tenant-scoped entities
        modelBuilder.Entity<Store>().HasQueryFilter(s => tenantId == null || s.TenantId == tenantId);
        modelBuilder.Entity<Staff>().HasQueryFilter(s => tenantId == null || s.TenantId == tenantId);
        modelBuilder.Entity<Customer>().HasQueryFilter(c => tenantId == null || c.TenantId == tenantId);
        modelBuilder.Entity<Category>().HasQueryFilter(c => tenantId == null || c.TenantId == tenantId);
        modelBuilder.Entity<Product>().HasQueryFilter(p => tenantId == null || p.TenantId == tenantId);
        modelBuilder.Entity<ProductVariant>().HasQueryFilter(v => tenantId == null || v.TenantId == tenantId);
        modelBuilder.Entity<PricingRule>().HasQueryFilter(p => tenantId == null || p.TenantId == tenantId);
        modelBuilder.Entity<Role>().HasQueryFilter(r => tenantId == null || r.TenantId == tenantId);
        modelBuilder.Entity<Promotion>().HasQueryFilter(p => tenantId == null || p.TenantId == tenantId);
        modelBuilder.Entity<GiftCard>().HasQueryFilter(g => tenantId == null || g.TenantId == tenantId);
        modelBuilder.Entity<AuditLog>().HasQueryFilter(a => tenantId == null || a.TenantId == tenantId);
        modelBuilder.Entity<TenantSubscription>().HasQueryFilter(s => tenantId == null || s.TenantId == tenantId);

        // Indirectly scoped entities - filtered via navigation paths
        modelBuilder.Entity<Store>().HasQueryFilter(s => tenantId == null || s.TenantId == tenantId);
        modelBuilder.Entity<Terminal>().HasQueryFilter(t => tenantId == null || t.Store.TenantId == tenantId);
        
        modelBuilder.Entity<Inventory>().HasQueryFilter(i => tenantId == null || i.Store.TenantId == tenantId);
        modelBuilder.Entity<Transaction>().HasQueryFilter(t => tenantId == null || t.Store.TenantId == tenantId);
        modelBuilder.Entity<TransactionItem>().HasQueryFilter(i => tenantId == null || i.Transaction.Store.TenantId == tenantId);
        modelBuilder.Entity<Payment>().HasQueryFilter(p => tenantId == null || p.Transaction.Store.TenantId == tenantId);
        modelBuilder.Entity<AppliedDiscount>().HasQueryFilter(d => tenantId == null || d.Transaction.Store.TenantId == tenantId);
        
        modelBuilder.Entity<EodReport>().HasQueryFilter(e => tenantId == null || e.Store.TenantId == tenantId);
        modelBuilder.Entity<Staff>().HasQueryFilter(s => tenantId == null || s.TenantId == tenantId);
        modelBuilder.Entity<TillSession>().HasQueryFilter(s => tenantId == null || s.Staff.TenantId == tenantId);
        
        modelBuilder.Entity<Customer>().HasQueryFilter(c => tenantId == null || c.TenantId == tenantId);
        modelBuilder.Entity<LoyaltyLedgerEntry>().HasQueryFilter(l => tenantId == null || l.Customer.TenantId == tenantId);
        
        modelBuilder.Entity<Category>().HasQueryFilter(c => tenantId == null || c.TenantId == tenantId);
        modelBuilder.Entity<Product>().HasQueryFilter(p => tenantId == null || p.TenantId == tenantId);
        modelBuilder.Entity<ProductVariant>().HasQueryFilter(v => tenantId == null || v.TenantId == tenantId);
        modelBuilder.Entity<PricingRule>().HasQueryFilter(p => tenantId == null || p.TenantId == tenantId);
        modelBuilder.Entity<Promotion>().HasQueryFilter(p => tenantId == null || p.TenantId == tenantId);
        modelBuilder.Entity<Coupon>().HasQueryFilter(c => tenantId == null || c.Promotion.TenantId == tenantId);
        modelBuilder.Entity<GiftCard>().HasQueryFilter(g => tenantId == null || g.TenantId == tenantId);
        modelBuilder.Entity<Role>().HasQueryFilter(r => tenantId == null || r.TenantId == tenantId);

        // ── Npgsql ENUM MAPPINGS ─────────────────────────────────────────────
        modelBuilder.HasPostgresEnum<TerminalStatus>();
        modelBuilder.HasPostgresEnum<SessionStatus>();
        modelBuilder.HasPostgresEnum<TaxCategory>();
        modelBuilder.HasPostgresEnum<TransactionType>();
        modelBuilder.HasPostgresEnum<TransactionStatus>();
        modelBuilder.HasPostgresEnum<PaymentMethod>();
        modelBuilder.HasPostgresEnum<PaymentStatus>();
        modelBuilder.HasPostgresEnum<DiscountType>();
        modelBuilder.HasPostgresEnum<PromotionType>();
        modelBuilder.HasPostgresEnum<PromotionScope>();
        modelBuilder.HasPostgresEnum<CustomerTier>();
        
        // SaaS Enums
        modelBuilder.HasPostgresEnum<SystemRole>();
        modelBuilder.HasPostgresEnum<SubscriptionPlan>();
        modelBuilder.HasPostgresEnum<SubscriptionStatus>();
        modelBuilder.HasPostgresEnum<BillingCycle>();
        modelBuilder.HasPostgresEnum<AuditAction>();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
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
