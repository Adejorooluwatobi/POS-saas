using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using POS.Application.DTOs;
using POS.Domain.Common;
using POS.Domain.Repositories;

namespace POS.Application.Queries.TillSession.GetPaged;

public class GetTillSessionsPagedQueryHandler : IRequestHandler<GetTillSessionsPagedQuery, PagedResult<TillSessionDto>>
{
    private readonly ITillSessionRepository _repository;
    private readonly IMapper _mapper;

    public GetTillSessionsPagedQueryHandler(ITillSessionRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PagedResult<TillSessionDto>> Handle(GetTillSessionsPagedQuery request, CancellationToken cancellationToken)
    {
        var pagedEntities = await _repository.GetPagedAsync(request.PageNumber, request.PageSize);
        
        return new PagedResult<TillSessionDto>
        {
            Items = _mapper.Map<IEnumerable<TillSessionDto>>(pagedEntities.Items),
            TotalCount = pagedEntities.TotalCount,
            PageNumber = pagedEntities.PageNumber,
            PageSize = pagedEntities.PageSize
        };
    }
}
