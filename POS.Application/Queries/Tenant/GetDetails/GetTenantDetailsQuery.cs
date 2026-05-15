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
    private readonly ITransactionRepository _transactionRepo;
    private readonly ITenantSubscriptionRepository _subscriptionRepo;
    private readonly ITenantContext _tenantContext;

    public GetTenantDetailsQueryHandler(
        ITenantRepository tenantRepo,
        IStoreRepository storeRepo,
        ITransactionRepository transactionRepo,
        ITenantSubscriptionRepository subscriptionRepo,
        ITenantContext tenantContext)
    {
        _tenantRepo = tenantRepo;
        _storeRepo = storeRepo;
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

        var tenant = await _tenantRepo.GetByIdAsync(request.Id)
            ?? throw new KeyNotFoundException($"Tenant {request.Id} not found.");

        var now = DateTimeOffset.UtcNow;
        var today = now.Date;
        var startOfWeek = now.AddDays(-(int)now.DayOfWeek + (int)DayOfWeek.Monday).Date;
        var startOfMonth = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, now.Offset);
        var startOfYear = new DateTimeOffset(now.Year, 1, 1, 0, 0, 0, now.Offset);

        // Revenue Base Query (Filtered by Tenant via Stores)
        // Note: Transaction has StoreId, and Store has TenantId.
        var transactions = _transactionRepo.GetQueryable()
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
                .Where(t => t.StoreId == store.Id && t.Status == TransactionStatus.Completed && t.CreatedAt >= startOfMonth)
                .SumAsync(t => t.GrandTotal, cancellationToken);
            store.LifetimeRevenue = await _transactionRepo.GetQueryable()
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

        return new TenantDetailsDto
        {
            Id = tenant.Id,
            BusinessName = tenant.BusinessName,
            Slug = tenant.Slug,
            ContactEmail = tenant.ContactEmail,
            IsActive = tenant.IsActive,
            Revenue = stats,
            Stores = stores,
            Subscription = subscriptionDto
        };
    }
}
