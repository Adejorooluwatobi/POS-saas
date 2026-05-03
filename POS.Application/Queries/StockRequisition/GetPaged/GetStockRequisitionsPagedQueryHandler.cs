using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using POS.Application.DTOs.StockRequisition;
using POS.Domain.Common;
using POS.Domain.Repositories;

namespace POS.Application.Queries.StockRequisition.GetPaged;

public class GetStockRequisitionsPagedQueryHandler : IRequestHandler<GetStockRequisitionsPagedQuery, PagedResult<StockRequisitionDto>>
{
    private readonly IStockRequisitionRepository _repository;
    private readonly IMapper _mapper;

    public GetStockRequisitionsPagedQueryHandler(IStockRequisitionRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PagedResult<StockRequisitionDto>> Handle(GetStockRequisitionsPagedQuery request, CancellationToken cancellationToken)
    {
        var pagedEntities = await _repository.GetPagedAsync(request.PageNumber, request.PageSize);
        
        return new PagedResult<StockRequisitionDto>
        {
            Items = _mapper.Map<IEnumerable<StockRequisitionDto>>(pagedEntities.Items),
            TotalCount = pagedEntities.TotalCount,
            PageNumber = pagedEntities.PageNumber,
            PageSize = pagedEntities.PageSize
        };
    }
}
