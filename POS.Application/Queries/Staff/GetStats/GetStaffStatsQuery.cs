using MediatR;
using Microsoft.EntityFrameworkCore;
using POS.Application.DTOs.Statistics;
using POS.Domain.Enums;
using POS.Domain.Repositories;

namespace POS.Application.Queries.Staff.GetStats;

public record GetStaffStatsQuery(Guid StaffId, int? Year = null, int? Month = null) : IRequest<RevenueStatsDto>;

public class GetStaffStatsQueryHandler : IRequestHandler<GetStaffStatsQuery, RevenueStatsDto>
{
    private readonly ITransactionRepository _transactionRepo;

    public GetStaffStatsQueryHandler(ITransactionRepository transactionRepo)
    {
        _transactionRepo = transactionRepo;
    }

    public async Task<RevenueStatsDto> Handle(GetStaffStatsQuery request, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var today = now.Date;
        var startOfWeek = now.AddDays(-(int)now.DayOfWeek + (int)DayOfWeek.Monday).Date;
        var startOfMonth = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, now.Offset);
        var startOfYear = new DateTimeOffset(now.Year, 1, 1, 0, 0, 0, now.Offset);

        var transactions = _transactionRepo.GetQueryable()
            .Where(t => t.CashierId == request.StaffId && t.Status == TransactionStatus.Completed);

        var stats = new RevenueStatsDto
        {
            Daily = await transactions.Where(t => t.CreatedAt >= today).SumAsync(t => t.GrandTotal, cancellationToken),
            Weekly = await transactions.Where(t => t.CreatedAt >= startOfWeek).SumAsync(t => t.GrandTotal, cancellationToken),
            Monthly = await transactions.Where(t => t.CreatedAt >= startOfMonth).SumAsync(t => t.GrandTotal, cancellationToken),
            Yearly = await transactions.Where(t => t.CreatedAt >= startOfYear).SumAsync(t => t.GrandTotal, cancellationToken),
            Lifetime = await transactions.SumAsync(t => t.GrandTotal, cancellationToken)
        };

        if (request.Year.HasValue)
        {
            var filterQuery = transactions.Where(t => t.CreatedAt.Year == request.Year.Value);
            if (request.Month.HasValue)
            {
                filterQuery = filterQuery.Where(t => t.CreatedAt.Month == request.Month.Value);
            }
            stats.FilteredRevenue = await filterQuery.SumAsync(t => t.GrandTotal, cancellationToken);
        }

        return stats;
    }
}
