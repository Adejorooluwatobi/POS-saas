using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using POS.Domain.Entities;
using POS.Domain.Repositories;
using POS.Infrastructure.Data;

namespace POS.Infrastructure.Repositories;

public class TenantRepository : GenericRepository<Tenant>, ITenantRepository
{
    public TenantRepository(RetailOsDbContext context) : base(context)
    {
    }

    public async Task PurgeTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        using var tx = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            // 1. Inventory & Requisitions
            await _context.StockMovements.IgnoreQueryFilters().Where(m => m.TenantId == tenantId).ExecuteDeleteAsync(cancellationToken);
            await _context.StockRequisitionItems.IgnoreQueryFilters().Where(i => i.Requisition.TenantId == tenantId).ExecuteDeleteAsync(cancellationToken);
            await _context.StockRequisitions.IgnoreQueryFilters().Where(r => r.TenantId == tenantId).ExecuteDeleteAsync(cancellationToken);
            await _context.InventoryOrderItems.IgnoreQueryFilters().Where(i => i.Order.TenantId == tenantId).ExecuteDeleteAsync(cancellationToken);
            await _context.InventoryOrders.IgnoreQueryFilters().Where(o => o.TenantId == tenantId).ExecuteDeleteAsync(cancellationToken);
            await _context.InventoryBatches.IgnoreQueryFilters().Where(b => b.TenantId == tenantId).ExecuteDeleteAsync(cancellationToken);
            await _context.Inventories.IgnoreQueryFilters().Where(i => i.TenantId == tenantId).ExecuteDeleteAsync(cancellationToken);

            // 2. Products, Overrides & Pricing Rules
            await _context.StoreProductOverrides.IgnoreQueryFilters().Where(o => o.TenantId == tenantId).ExecuteDeleteAsync(cancellationToken);
            await _context.ProductBarcodes.IgnoreQueryFilters().Where(b => b.TenantId == tenantId).ExecuteDeleteAsync(cancellationToken);
            await _context.PricingRules.IgnoreQueryFilters().Where(pr => pr.TenantId == tenantId).ExecuteDeleteAsync(cancellationToken);
            await _context.ProductVariants.IgnoreQueryFilters().Where(v => v.TenantId == tenantId).ExecuteDeleteAsync(cancellationToken);
            await _context.Products.IgnoreQueryFilters().Where(p => p.TenantId == tenantId).ExecuteDeleteAsync(cancellationToken);
            await _context.Categories.IgnoreQueryFilters().Where(c => c.TenantId == tenantId).ExecuteDeleteAsync(cancellationToken);

            // 3. Transactions, Items, Payments & Discounts
            await _context.AppliedDiscounts.IgnoreQueryFilters().Where(ad => ad.Transaction.Store.TenantId == tenantId).ExecuteDeleteAsync(cancellationToken);
            await _context.Payments.IgnoreQueryFilters().Where(p => p.Transaction.Store.TenantId == tenantId).ExecuteDeleteAsync(cancellationToken);
            await _context.TransactionItems.IgnoreQueryFilters().Where(ti => ti.Transaction.Store.TenantId == tenantId).ExecuteDeleteAsync(cancellationToken);
            await _context.Transactions.IgnoreQueryFilters().Where(t => t.Store.TenantId == tenantId).ExecuteDeleteAsync(cancellationToken);

            // 4. Hardware, Shifts & Reports
            await _context.TillSessions.IgnoreQueryFilters().Where(ts => ts.Staff.TenantId == tenantId).ExecuteDeleteAsync(cancellationToken);
            await _context.Terminals.IgnoreQueryFilters().Where(t => t.Store.TenantId == tenantId).ExecuteDeleteAsync(cancellationToken);
            await _context.EodReports.IgnoreQueryFilters().Where(e => e.Store.TenantId == tenantId).ExecuteDeleteAsync(cancellationToken);

            // 5. Loyalty, Gift Cards & Promotions
            await _context.GiftCardTransactions.IgnoreQueryFilters().Where(t => t.TenantId == tenantId).ExecuteDeleteAsync(cancellationToken);
            await _context.GiftCards.IgnoreQueryFilters().Where(g => g.TenantId == tenantId).ExecuteDeleteAsync(cancellationToken);
            await _context.LoyaltyLedger.IgnoreQueryFilters().Where(l => l.Customer.TenantId == tenantId).ExecuteDeleteAsync(cancellationToken);
            await _context.Customers.IgnoreQueryFilters().Where(c => c.TenantId == tenantId).ExecuteDeleteAsync(cancellationToken);
            await _context.Coupons.IgnoreQueryFilters().Where(c => c.Promotion.TenantId == tenantId).ExecuteDeleteAsync(cancellationToken);
            await _context.Promotions.IgnoreQueryFilters().Where(p => p.TenantId == tenantId).ExecuteDeleteAsync(cancellationToken);

            // 6. Audit Logs, Staff & Roles
            await _context.AuditLogs.IgnoreQueryFilters().Where(a => a.TenantId == tenantId).ExecuteDeleteAsync(cancellationToken);
            await _context.Staff.IgnoreQueryFilters().Where(s => s.TenantId == tenantId).ExecuteDeleteAsync(cancellationToken);
            await _context.Roles.IgnoreQueryFilters().Where(r => r.TenantId == tenantId).ExecuteDeleteAsync(cancellationToken);

            // 7. Stores & Subscriptions
            await _context.Stores.IgnoreQueryFilters().Where(s => s.TenantId == tenantId).ExecuteDeleteAsync(cancellationToken);
            await _context.TenantSubscriptions.IgnoreQueryFilters().Where(sub => sub.TenantId == tenantId).ExecuteDeleteAsync(cancellationToken);

            // 8. Tenant entity
            await _context.Tenants.IgnoreQueryFilters().Where(t => t.Id == tenantId).ExecuteDeleteAsync(cancellationToken);

            await tx.CommitAsync(cancellationToken);
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
