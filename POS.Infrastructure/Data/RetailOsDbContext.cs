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
    public DbSet<StoreProductOverride> StoreProductOverrides => Set<StoreProductOverride>();
    public DbSet<ProductBarcode> ProductBarcodes => Set<ProductBarcode>();
    public DbSet<InventoryOrder> InventoryOrders => Set<InventoryOrder>();
    public DbSet<InventoryOrderItem> InventoryOrderItems => Set<InventoryOrderItem>();
    public DbSet<StockRequisition> StockRequisitions => Set<StockRequisition>();
    public DbSet<StockRequisitionItem> StockRequisitionItems => Set<StockRequisitionItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all IEntityTypeConfiguration<T> in this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RetailOsDbContext).Assembly);

        // ── GLOBAL QUERY FILTERS (Multi-Tenancy) ─────────────────────────────
        // Tenant can only see their own record (SuperAdmin sees all)
        modelBuilder.Entity<Tenant>().HasQueryFilter(t => _tenant.TenantId == null || t.Id == _tenant.TenantId);

        // Core tenant-scoped entities
        modelBuilder.Entity<Store>().HasQueryFilter(s => (_tenant.TenantId == null || s.TenantId == _tenant.TenantId) && (_tenant.StoreId == null || s.Id == _tenant.StoreId));
        modelBuilder.Entity<Staff>().HasQueryFilter(s => (_tenant.TenantId == null || s.TenantId == _tenant.TenantId) && (_tenant.StoreId == null || s.StoreId == _tenant.StoreId));
        modelBuilder.Entity<Customer>().HasQueryFilter(c => _tenant.TenantId == null || c.TenantId == _tenant.TenantId);
        modelBuilder.Entity<Category>().HasQueryFilter(c => _tenant.TenantId == null || c.TenantId == _tenant.TenantId);
        modelBuilder.Entity<Product>().HasQueryFilter(p => (_tenant.TenantId == null || p.TenantId == _tenant.TenantId) && (p.StoreId == null || _tenant.StoreId == null || p.StoreId == _tenant.StoreId || _tenant.SystemRole == "SuperAdmin" || _tenant.SystemRole == "TenantAdmin" || _tenant.SystemRole == "Manager"));
        modelBuilder.Entity<ProductVariant>().HasQueryFilter(v => _tenant.TenantId == null || v.TenantId == _tenant.TenantId);
        modelBuilder.Entity<PricingRule>().HasQueryFilter(p => _tenant.TenantId == null || p.TenantId == _tenant.TenantId);
        modelBuilder.Entity<Role>().HasQueryFilter(r => _tenant.TenantId == null || r.TenantId == _tenant.TenantId);
        modelBuilder.Entity<Promotion>().HasQueryFilter(p => _tenant.TenantId == null || p.TenantId == _tenant.TenantId);
        modelBuilder.Entity<GiftCard>().HasQueryFilter(g => _tenant.TenantId == null || g.TenantId == _tenant.TenantId);
        modelBuilder.Entity<AuditLog>().HasQueryFilter(a => _tenant.TenantId == null || a.TenantId == _tenant.TenantId);
        modelBuilder.Entity<TenantSubscription>().HasQueryFilter(s => _tenant.TenantId == null || s.TenantId == _tenant.TenantId);
        modelBuilder.Entity<StoreProductOverride>().HasQueryFilter(o => (_tenant.TenantId == null || o.TenantId == _tenant.TenantId) && (_tenant.StoreId == null || o.StoreId == _tenant.StoreId));
        modelBuilder.Entity<ProductBarcode>().HasQueryFilter(b => (_tenant.TenantId == null || b.TenantId == _tenant.TenantId) && (_tenant.StoreId == null || b.StoreId == _tenant.StoreId));

        // Indirectly scoped entities - filtered via navigation paths
        modelBuilder.Entity<Terminal>().HasQueryFilter(t => (_tenant.TenantId == null || t.Store.TenantId == _tenant.TenantId) && (_tenant.StoreId == null || t.StoreId == _tenant.StoreId));
        
        modelBuilder.Entity<Inventory>().HasQueryFilter(i => (_tenant.TenantId == null || i.Store.TenantId == _tenant.TenantId) && (_tenant.StoreId == null || i.StoreId == _tenant.StoreId));
        modelBuilder.Entity<Transaction>().HasQueryFilter(t => (_tenant.TenantId == null || t.Store.TenantId == _tenant.TenantId) && (_tenant.StoreId == null || t.StoreId == _tenant.StoreId));
        modelBuilder.Entity<TransactionItem>().HasQueryFilter(i => (_tenant.TenantId == null || i.Transaction.Store.TenantId == _tenant.TenantId) && (_tenant.StoreId == null || i.Transaction.StoreId == _tenant.StoreId));
        modelBuilder.Entity<Payment>().HasQueryFilter(p => (_tenant.TenantId == null || p.Transaction.Store.TenantId == _tenant.TenantId) && (_tenant.StoreId == null || p.Transaction.StoreId == _tenant.StoreId));
        modelBuilder.Entity<AppliedDiscount>().HasQueryFilter(d => (_tenant.TenantId == null || d.Transaction.Store.TenantId == _tenant.TenantId) && (_tenant.StoreId == null || d.Transaction.StoreId == _tenant.StoreId));
        
        modelBuilder.Entity<EodReport>().HasQueryFilter(e => (_tenant.TenantId == null || e.Store.TenantId == _tenant.TenantId) && (_tenant.StoreId == null || e.StoreId == _tenant.StoreId));
        modelBuilder.Entity<TillSession>().HasQueryFilter(s => (_tenant.TenantId == null || s.Staff.TenantId == _tenant.TenantId) && (_tenant.StoreId == null || s.Terminal.StoreId == _tenant.StoreId));
        
        modelBuilder.Entity<LoyaltyLedgerEntry>().HasQueryFilter(l => _tenant.TenantId == null || l.Customer.TenantId == _tenant.TenantId);
        
        modelBuilder.Entity<Coupon>().HasQueryFilter(c => _tenant.TenantId == null || c.Promotion.TenantId == _tenant.TenantId);

        modelBuilder.Entity<InventoryOrder>().HasQueryFilter(o =>
            (_tenant.TenantId == null || o.TenantId == _tenant.TenantId) &&
            (_tenant.StoreId == null ||
             o.DestinationStoreId == _tenant.StoreId ||
             o.SourceStoreId == _tenant.StoreId));

        modelBuilder.Entity<StockRequisition>().HasQueryFilter(r =>
            (_tenant.TenantId == null || r.TenantId == _tenant.TenantId) &&
            (_tenant.StoreId == null || r.RequestingStoreId == _tenant.StoreId));

        modelBuilder.Entity<StockRequisitionItem>().HasQueryFilter(i =>
            (_tenant.TenantId == null || i.Requisition.TenantId == _tenant.TenantId) &&
            (_tenant.StoreId == null || i.Requisition.RequestingStoreId == _tenant.StoreId));

        modelBuilder.Entity<InventoryOrderItem>().HasQueryFilter(i =>
            (_tenant.TenantId == null || i.Order.TenantId == _tenant.TenantId) &&
            (_tenant.StoreId == null || i.Order.DestinationStoreId == _tenant.StoreId || i.Order.SourceStoreId == _tenant.StoreId));

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

        modelBuilder.HasPostgresEnum<InventoryOrderType>();
        modelBuilder.HasPostgresEnum<InventoryOrderStatus>();
        modelBuilder.HasPostgresEnum<RequisitionStatus>();
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
