using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using POS.Domain.Enums;
using POS.Domain.Repositories;

namespace POS.Application.Queries.Tenant.ExportData;

public class ExportTenantDataQueryHandler : IRequestHandler<ExportTenantDataQuery, TenantExportResultDto>
{
    private readonly ITenantRepository _tenantRepo;
    private readonly IStoreRepository _storeRepo;
    private readonly IStaffRepository _staffRepo;
    private readonly IProductRepository _productRepo;
    private readonly ITransactionRepository _transactionRepo;
    private readonly ITenantSubscriptionRepository _subRepo;

    public ExportTenantDataQueryHandler(
        ITenantRepository tenantRepo,
        IStoreRepository storeRepo,
        IStaffRepository staffRepo,
        IProductRepository productRepo,
        ITransactionRepository transactionRepo,
        ITenantSubscriptionRepository subRepo)
    {
        _tenantRepo = tenantRepo;
        _storeRepo = storeRepo;
        _staffRepo = staffRepo;
        _productRepo = productRepo;
        _transactionRepo = transactionRepo;
        _subRepo = subRepo;
    }

    public async Task<TenantExportResultDto> Handle(ExportTenantDataQuery request, CancellationToken cancellationToken)
    {
        var tenant = await _tenantRepo.GetQueryable()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Tenant {request.Id} not found.");

        var sub = await _subRepo.GetByTenantAsync(tenant.Id);

        var stores = await _storeRepo.GetQueryable()
            .IgnoreQueryFilters()
            .Where(s => s.TenantId == tenant.Id)
            .ToListAsync(cancellationToken);

        var staff = await _staffRepo.GetQueryable()
            .IgnoreQueryFilters()
            .Include(s => s.Store)
            .Where(s => s.TenantId == tenant.Id)
            .ToListAsync(cancellationToken);

        var products = await _productRepo.GetQueryable()
            .IgnoreQueryFilters()
            .Include(p => p.Category)
            .Include(p => p.Variants)
            .Where(p => p.TenantId == tenant.Id)
            .ToListAsync(cancellationToken);

        var transactions = await _transactionRepo.GetQueryable()
            .IgnoreQueryFilters()
            .Where(t => t.Store.TenantId == tenant.Id)
            .ToListAsync(cancellationToken);

        var sb = new StringBuilder();

        // Section 1: Tenant Information
        sb.AppendLine("=== TENANT INFORMATION ===");
        sb.AppendLine("Field,Value");
        sb.AppendLine($"Business Name,\"{Escape(tenant.BusinessName)}\"");
        sb.AppendLine($"Slug,\"{Escape(tenant.Slug)}\"");
        sb.AppendLine($"Contact Email,\"{Escape(tenant.ContactEmail)}\"");
        sb.AppendLine($"Contact Phone,\"{Escape(tenant.ContactPhone ?? "")}\"");
        sb.AppendLine($"Country,\"{Escape(tenant.Country)}\"");
        sb.AppendLine($"Status,{(tenant.IsActive ? "Active" : "Suspended")}");
        sb.AppendLine($"In Recycle Bin,{(tenant.IsDeleted ? "Yes" : "No")}");
        sb.AppendLine($"Registered At,{tenant.CreatedAt:yyyy-MM-dd HH:mm:ss UTC}");
        sb.AppendLine($"Deleted At,{(tenant.DeletedAt.HasValue ? tenant.DeletedAt.Value.ToString("yyyy-MM-dd HH:mm:ss UTC") : "N/A")}");
        sb.AppendLine($"Plan Tier,{(sub != null ? sub.Plan.ToString() : "N/A")}");
        sb.AppendLine($"Plan Expiry,{(sub != null ? sub.CurrentPeriodEnd.ToString("yyyy-MM-dd") : "N/A")}");
        sb.AppendLine($"Store Quota,{(sub != null ? sub.MaxStores.ToString() : "N/A")}");
        sb.AppendLine($"Staff Quota,{(sub != null ? sub.MaxStaff.ToString() : "N/A")}");
        sb.AppendLine($"Terminal Quota,{(sub != null ? sub.MaxTerminals.ToString() : "N/A")}");
        sb.AppendLine();

        // Section 2: Stores
        sb.AppendLine("=== STORES & BRANCHES ===");
        sb.AppendLine("Code,Store Name,City,State,Address,Phone,Status");
        foreach (var s in stores)
        {
            sb.AppendLine($"\"{Escape(s.Code)}\",\"{Escape(s.Name)}\",\"{Escape(s.City)}\",\"{Escape(s.State ?? "")}\",\"{Escape(s.Address)}\",\"{Escape(s.Phone ?? "")}\",{(s.IsActive ? "Active" : "Inactive")}");
        }
        sb.AppendLine();

        // Section 3: Staff Roster
        sb.AppendLine("=== STAFF ROSTER ===");
        sb.AppendLine("Employee No,Full Name,Email,System Role,Assigned Store,Hire Date,Status");
        foreach (var st in staff)
        {
            var storeName = st.Store?.Name ?? (st.SystemRole == SystemRole.TenantAdmin ? "All Stores (HQ)" : "Unassigned");
            sb.AppendLine($"\"{Escape(st.EmployeeNo)}\",\"{Escape(st.FullName)}\",\"{Escape(st.Email)}\",\"{st.SystemRole}\",\"{Escape(storeName)}\",{st.HiredAt:yyyy-MM-dd},{(st.IsActive ? "Active" : "Suspended")}");
        }
        sb.AppendLine();

        // Section 4: Products Catalog
        sb.AppendLine("=== PRODUCT CATALOG ===");
        sb.AppendLine("Master SKU,Product Name,Category,Variant SKU,Barcode,Base Price,Unit,Active");
        foreach (var p in products)
        {
            if (p.Variants.Any())
            {
                foreach (var v in p.Variants)
                {
                    sb.AppendLine($"\"{Escape(p.MasterSku)}\",\"{Escape(p.Name)}\",\"{Escape(p.Category?.Name ?? "Uncategorized")}\",\"{Escape(v.Sku)}\",\"{Escape(v.Barcode)}\",{v.BasePrice.ToString("F2", CultureInfo.InvariantCulture)},\"{Escape(v.UnitOfMeasure)}\",{(v.IsActive ? "Active" : "Inactive")}");
                }
            }
            else
            {
                sb.AppendLine($"\"{Escape(p.MasterSku)}\",\"{Escape(p.Name)}\",\"{Escape(p.Category?.Name ?? "Uncategorized")}\",\"N/A\",\"N/A\",0.00,\"Each\",{(p.IsActive ? "Active" : "Inactive")}");
            }
        }
        sb.AppendLine();

        // Section 5: Transactions & Revenue Summary
        sb.AppendLine("=== SALES & TRANSACTION SUMMARY ===");
        var totalTx = transactions.Count;
        var completedTx = transactions.Count(t => t.Status == TransactionStatus.Completed);
        var totalRevenue = transactions.Where(t => t.Status == TransactionStatus.Completed).Sum(t => t.GrandTotal);
        sb.AppendLine("Metric,Value");
        sb.AppendLine($"Total Transactions Processed,{totalTx}");
        sb.AppendLine($"Completed Transactions,{completedTx}");
        sb.AppendLine($"Lifetime Revenue (NGN),{totalRevenue.ToString("F2", CultureInfo.InvariantCulture)}");

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        var fileName = $"Tenant_{tenant.Slug}_Export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";

        return new TenantExportResultDto
        {
            FileName = fileName,
            CsvBytes = bytes,
            ContentType = "text/csv; charset=utf-8"
        };
    }

    private static string Escape(string value)
    {
        return value.Replace("\"", "\"\"");
    }
}
