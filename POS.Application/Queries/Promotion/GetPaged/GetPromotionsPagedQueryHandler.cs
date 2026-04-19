using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using POS.Application.DTOs;
using POS.Domain.Common;
using POS.Domain.Repositories;

namespace POS.Application.Queries.Promotion.GetPaged;

public class GetPromotionsPagedQueryHandler : IRequestHandler<GetPromotionsPagedQuery, PagedResult<PromotionDto>>
{
    private readonly IPromotionRepository _repository;
    private readonly IMapper _mapper;

    public GetPromotionsPagedQueryHandler(IPromotionRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PagedResult<PromotionDto>> Handle(GetPromotionsPagedQuery request, CancellationToken cancellationToken)
    {
        var pagedEntities = await _repository.GetPagedAsync(request.PageNumber, request.PageSize);
        
        return new PagedResult<PromotionDto>
        {
            Items = _mapper.Map<IEnumerable<PromotionDto>>(pagedEntities.Items),
            TotalCount = pagedEntities.TotalCount,
            PageNumber = pagedEntities.PageNumber,
            PageSize = pagedEntities.PageSize
        };
    }
}
