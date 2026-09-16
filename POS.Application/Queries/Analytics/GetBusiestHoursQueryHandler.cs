using MediatR;
using Microsoft.EntityFrameworkCore;
using POS.Domain.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using POS.Domain.Enums;

namespace POS.Application.Queries.Analytics;

public class GetBusiestHoursQueryHandler : IRequestHandler<GetBusiestHoursQuery, List<BusiestHourDto>>
{
    private readonly ITransactionRepository _transactionRepository;

    public GetBusiestHoursQueryHandler(ITransactionRepository transactionRepository)
    {
        _transactionRepository = transactionRepository;
    }

    public async Task<List<BusiestHourDto>> Handle(GetBusiestHoursQuery request, CancellationToken cancellationToken)
    {
        var query = _transactionRepository.GetQueryable()
            .Where(t => t.Status == TransactionStatus.Completed);

        if (request.StoreId.HasValue)
        {
            query = query.Where(t => t.StoreId == request.StoreId.Value);
        }
        
        if (request.StartDate.HasValue)
        {
            query = query.Where(t => t.CreatedAt >= request.StartDate.Value);
        }
        
        if (request.EndDate.HasValue)
        {
            query = query.Where(t => t.CreatedAt <= request.EndDate.Value);
        }

        var results = await query
            .GroupBy(t => t.CreatedAt.Hour)
            .Select(g => new BusiestHourDto
            {
                HourOfDay = g.Key,
                TransactionCount = g.Count(),
                TotalRevenue = g.Sum(t => t.GrandTotal)
            })
            .OrderBy(r => r.HourOfDay)
            .ToListAsync(cancellationToken);

        return results;
    }
}
