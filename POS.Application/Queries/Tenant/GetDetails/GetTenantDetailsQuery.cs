using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using POS.Application.DTOs.Statistics;
using POS.Domain.Enums;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;

namespace POS.Application.Queries.Tenant.GetDetails;

public record GetTenantDetailsQuery(Guid Id, int? Year = null, int? Month = null) : IRequest<TenantDetailsDto>;

public class GetTenantDetailsQueryHandler : IRequestHandler<GetTenantDetailsQuery, TenantDetailsDto>
{
    private readonly ITenantRepository _tenantRepo;
    private readonly IStoreRepository _storeRepo;
    private readonly IStaffRepository _staffRepo;
    private readonly ITransactionRepository _transactionRepo;
    private readonly ITenantSubscriptionRepository _subscriptionRepo;
    private readonly ITenantContext _tenantContext;

    public GetTenantDetailsQueryHandler(
        ITenantRepository tenantRepo,
        IStoreRepository storeRepo,
        IStaffRepository staffRepo,
        ITransactionRepository transactionRepo,
        ITenantSubscriptionRepository subscriptionRepo,
        ITenantContext tenantContext)
    {
        _tenantRepo = tenantRepo;
        _storeRepo = storeRepo;
        _staffRepo = staffRepo;
        _transactionRepo = transactionRepo;
        _subscriptionRepo = subscriptionRepo;
        _tenantContext = tenantContext;
    }

    public async Task<TenantDetailsDto> Handle(GetTenantDetailsQuery request, CancellationToken cancellationToken)
    {
        if (_tenantContext.SystemRole != "SuperAdmin" && _tenantContext.TenantId != request.Id)
        {
            throw new UnauthorizedAccessException("You are not authorized to view details for this organization.");
        }

        var tenant = await _tenantRepo.GetQueryable()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Tenant {request.Id} not found.");

        var now = DateTimeOffset.UtcNow;
        var today = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, TimeSpan.Zero);
        int diff = (7 + (now.DayOfWeek - DayOfWeek.Monday)) % 7;
        var startOfWeekDate = now.AddDays(-diff);
        var startOfWeek = new DateTimeOffset(startOfWeekDate.Year, startOfWeekDate.Month, startOfWeekDate.Day, 0, 0, 0, TimeSpan.Zero);
        var startOfMonth = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);
        var startOfYear = new DateTimeOffset(now.Year, 1, 1, 0, 0, 0, TimeSpan.Zero);

        // Revenue Base Query (Filtered by Tenant via Stores)
        var transactions = _transactionRepo.GetQueryable()
            .IgnoreQueryFilters()
            .Include(t => t.Store)
            .Where(t => t.Store.TenantId == request.Id && t.Status == TransactionStatus.Completed);

        var stats = new RevenueStatsDto
        {
            Daily = await transactions.Where(t => t.CreatedAt >= today).SumAsync(t => t.GrandTotal, cancellationToken),
            Weekly = await transactions.Where(t => t.CreatedAt >= startOfWeek).SumAsync(t => t.GrandTotal, cancellationToken),
            Monthly = await transactions.Where(t => t.CreatedAt >= startOfMonth).SumAsync(t => t.GrandTotal, cancellationToken),
            Yearly = await transactions.Where(t => t.CreatedAt >= startOfYear).SumAsync(t => t.GrandTotal, cancellationToken),
            Lifetime = await transactions.SumAsync(t => t.GrandTotal, cancellationToken)
        };

        // Specific Filtering
        if (request.Year.HasValue)
        {
            var filterQuery = transactions.Where(t => t.CreatedAt.Year == request.Year.Value);
            if (request.Month.HasValue)
            {
                filterQuery = filterQuery.Where(t => t.CreatedAt.Month == request.Month.Value);
            }
            stats.FilteredRevenue = await filterQuery.SumAsync(t => t.GrandTotal, cancellationToken);
        }

        // Stores list with their summary stats
        var stores = await _storeRepo.GetQueryable()
            .IgnoreQueryFilters()
            .Where(s => s.TenantId == request.Id)
            .Select(s => new StoreSummaryDto
            {
                Id = s.Id,
                Name = s.Name,
                Code = s.Code,
                IsActive = s.IsActive
            })
            .ToListAsync(cancellationToken);

        foreach (var store in stores)
        {
            store.MonthlyRevenue = await _transactionRepo.GetQueryable()
                .IgnoreQueryFilters()
                .Where(t => t.StoreId == store.Id && t.Status == TransactionStatus.Completed && t.CreatedAt >= startOfMonth)
                .SumAsync(t => t.GrandTotal, cancellationToken);
            store.LifetimeRevenue = await _transactionRepo.GetQueryable()
                .IgnoreQueryFilters()
                .Where(t => t.StoreId == store.Id && t.Status == TransactionStatus.Completed)
                .SumAsync(t => t.GrandTotal, cancellationToken);
        }

        // Fetch subscription
        var subscription = await _subscriptionRepo.GetByTenantAsync(request.Id);
        TenantSubscriptionSummaryDto? subscriptionDto = subscription == null ? null : new TenantSubscriptionSummaryDto
        {
            Id = subscription.Id,
            Plan = (int)subscription.Plan,
            Status = (int)subscription.Status,
            BillingCycle = (int)subscription.BillingCycle,
            MaxStores = subscription.MaxStores,
            MaxStaff = subscription.MaxStaff,
            MaxTerminals = subscription.MaxTerminals,
            MonthlyPrice = subscription.MonthlyPrice,
            CurrentPeriodStart = subscription.CurrentPeriodStart,
            CurrentPeriodEnd = subscription.CurrentPeriodEnd
        };

        var adminStaff = await _staffRepo.GetQueryable()
            .IgnoreQueryFilters()
            .Where(s => s.TenantId == request.Id && s.SystemRole == SystemRole.TenantAdmin)
            .FirstOrDefaultAsync(cancellationToken);

        return new TenantDetailsDto
        {
            Id = tenant.Id,
            BusinessName = tenant.BusinessName,
            Slug = tenant.Slug,
            ContactEmail = tenant.ContactEmail,
            IsActive = tenant.IsActive,
            IsDeleted = tenant.IsDeleted,
            DeletedAt = tenant.DeletedAt,
            OwnerEmail = adminStaff?.Email,
            OwnerName = adminStaff?.FullName,
            OwnerStaffId = adminStaff?.Id,
            Revenue = stats,
            Stores = stores,
            Subscription = subscriptionDto
        };
    }
}
