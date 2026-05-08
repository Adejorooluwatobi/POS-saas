using MediatR;
using Microsoft.EntityFrameworkCore;
using POS.Application.DTOs.Statistics;
using POS.Domain.Enums;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;

namespace POS.Application.Queries.Store.GetDetails;

public record GetStoreDetailsQuery(Guid Id, int? Year = null, int? Month = null) : IRequest<StoreDetailsDto>;

public class GetStoreDetailsQueryHandler : IRequestHandler<GetStoreDetailsQuery, StoreDetailsDto>
{
    private readonly IStoreRepository _storeRepo;
    private readonly IStaffRepository _staffRepo;
    private readonly ITransactionRepository _transactionRepo;
    private readonly ITenantContext _tenantContext;

    public GetStoreDetailsQueryHandler(
        IStoreRepository storeRepo,
        IStaffRepository staffRepo,
        ITransactionRepository transactionRepo,
        ITenantContext tenantContext)
    {
        _storeRepo = storeRepo;
        _staffRepo = staffRepo;
        _transactionRepo = transactionRepo;
        _tenantContext = tenantContext;
    }

    public async Task<StoreDetailsDto> Handle(GetStoreDetailsQuery request, CancellationToken cancellationToken)
    {
        var store = await _storeRepo.GetByIdAsync(request.Id)
            ?? throw new KeyNotFoundException($"Store {request.Id} not found.");

        if (_tenantContext.SystemRole != "SuperAdmin" && _tenantContext.TenantId != store.TenantId)
        {
            throw new UnauthorizedAccessException("You are not authorized to view details for this store.");
        }

        var now = DateTimeOffset.UtcNow;
        var today = now.Date;
        var startOfWeek = now.AddDays(-(int)now.DayOfWeek + (int)DayOfWeek.Monday).Date;
        var startOfMonth = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, now.Offset);
        var startOfYear = new DateTimeOffset(now.Year, 1, 1, 0, 0, 0, now.Offset);

        // Revenue Base Query
        var transactions = _transactionRepo.GetQueryable()
            .Where(t => t.StoreId == request.Id && t.Status == TransactionStatus.Completed);

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

        // Staff stats
        var staffList = await _staffRepo.GetQueryable()
            .Where(s => s.StoreId == request.Id)
            .Select(s => new StaffStatsDto
            {
                Id = s.Id,
                FullName = s.FullName,
                EmployeeNo = s.EmployeeNo,
                SystemRole = s.SystemRole.ToString(),
                IsActive = s.IsActive,
                // We'll calculate their revenue separately or via subquery
            })
            .ToListAsync(cancellationToken);

        foreach (var staff in staffList)
        {
            staff.TodayRevenue = await transactions
                .Where(t => t.CashierId == staff.Id && t.CreatedAt >= today)
                .SumAsync(t => t.GrandTotal, cancellationToken);
            staff.LifetimeRevenue = await transactions
                .Where(t => t.CashierId == staff.Id)
                .SumAsync(t => t.GrandTotal, cancellationToken);
        }

        return new StoreDetailsDto
        {
            Id = store.Id,
            Name = store.Name,
            Code = store.Code,
            Address = store.Address,
            City = store.City,
            IsActive = store.IsActive,
            Revenue = stats,
            Staff = staffList
        };
    }
}
