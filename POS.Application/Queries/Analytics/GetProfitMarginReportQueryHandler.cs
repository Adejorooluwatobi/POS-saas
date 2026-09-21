using MediatR;
using Microsoft.EntityFrameworkCore;
using POS.Domain.Repositories;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using POS.Domain.Enums;

namespace POS.Application.Queries.Analytics;

public class GetProfitMarginReportQueryHandler : IRequestHandler<GetProfitMarginReportQuery, ProfitMarginReportDto>
{
    private readonly ITransactionRepository _transactionRepository;

    public GetProfitMarginReportQueryHandler(ITransactionRepository transactionRepository)
    {
        _transactionRepository = transactionRepository;
    }

    public async Task<ProfitMarginReportDto> Handle(GetProfitMarginReportQuery request, CancellationToken cancellationToken)
    {
        var transactionQuery = _transactionRepository.GetQueryable()
            .Where(t => t.Status == TransactionStatus.Completed);

        if (request.StoreId.HasValue)
        {
            transactionQuery = transactionQuery.Where(t => t.StoreId == request.StoreId.Value);
        }
        
        if (request.StartDate.HasValue)
        {
            transactionQuery = transactionQuery.Where(t => t.CreatedAt >= request.StartDate.Value);
        }
        
        if (request.EndDate.HasValue)
        {
            transactionQuery = transactionQuery.Where(t => t.CreatedAt <= request.EndDate.Value);
        }

        var itemQuery = transactionQuery
            .SelectMany(t => t.Items)
            .Where(ti => !ti.IsVoided);

        var stats = await itemQuery
            .GroupBy(ti => 1) // Group by constant to get overall sums
            .Select(g => new
            {
                Revenue = g.Sum(ti => ti.LineTotal),
                COGS = g.Sum(ti => ti.Quantity * ti.UnitCost)
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (stats == null)
        {
            return new ProfitMarginReportDto();
        }

        var profit = stats.Revenue - stats.COGS;
        var margin = stats.Revenue > 0 ? (profit / stats.Revenue) * 100 : 0;

        return new ProfitMarginReportDto
        {
            TotalRevenue = stats.Revenue,
            TotalCostOfGoodsSold = stats.COGS,
            GrossProfit = profit,
            ProfitMarginPercentage = margin
        };
    }
}
