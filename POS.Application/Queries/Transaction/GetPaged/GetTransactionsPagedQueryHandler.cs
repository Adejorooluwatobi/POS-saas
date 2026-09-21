using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using POS.Application.DTOs;
using POS.Domain.Common;
using POS.Domain.Repositories;

namespace POS.Application.Queries.Transaction.GetPaged;

public class GetTransactionsPagedQueryHandler : IRequestHandler<GetTransactionsPagedQuery, PagedResult<TransactionDto>>
{
    private readonly ITransactionRepository _repository;
    private readonly IMapper _mapper;

    public GetTransactionsPagedQueryHandler(ITransactionRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PagedResult<TransactionDto>> Handle(GetTransactionsPagedQuery request, CancellationToken cancellationToken)
    {
        var query = _repository.GetQueryable();
        
        if (request.CashierId.HasValue)
        {
            query = query.Where(t => t.CashierId == request.CashierId.Value);
        }
        
        var totalCount = await query.CountAsync(cancellationToken);
        
        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Include(t => t.Items)
            .Include(t => t.Payments)
            .ToListAsync(cancellationToken);

        return new PagedResult<TransactionDto>
        {
            Items = _mapper.Map<IEnumerable<TransactionDto>>(items),
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
