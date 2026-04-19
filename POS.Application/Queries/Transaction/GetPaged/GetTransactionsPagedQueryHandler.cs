using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
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
        var pagedEntities = await _repository.GetPagedAsync(request.PageNumber, request.PageSize);
        
        return new PagedResult<TransactionDto>
        {
            Items = _mapper.Map<IEnumerable<TransactionDto>>(pagedEntities.Items),
            TotalCount = pagedEntities.TotalCount,
            PageNumber = pagedEntities.PageNumber,
            PageSize = pagedEntities.PageSize
        };
    }
}
