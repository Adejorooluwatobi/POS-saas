using MediatR;
using Microsoft.EntityFrameworkCore;
using POS.Domain.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using POS.Domain.Enums;

namespace POS.Application.Queries.Analytics;

public class GetTopSellingProductsQueryHandler : IRequestHandler<GetTopSellingProductsQuery, List<TopSellingProductDto>>
{
    private readonly ITransactionRepository _transactionRepository;

    public GetTopSellingProductsQueryHandler(ITransactionRepository transactionRepository)
    {
        _transactionRepository = transactionRepository;
    }

    public async Task<List<TopSellingProductDto>> Handle(GetTopSellingProductsQuery request, CancellationToken cancellationToken)
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

        var results = await itemQuery
            .GroupBy(ti => new { ti.VariantId, ti.ProductName })
            .Select(g => new TopSellingProductDto
            {
                VariantId = g.Key.VariantId,
                ProductName = g.Key.ProductName,
                TotalQuantitySold = g.Sum(ti => ti.Quantity),
                TotalRevenue = g.Sum(ti => ti.LineTotal)
            })
            .OrderByDescending(r => r.TotalQuantitySold)
            .Take(request.Limit)
            .ToListAsync(cancellationToken);

        return results;
    }
}
