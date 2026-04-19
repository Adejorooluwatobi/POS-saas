using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using POS.Application.DTOs;
using POS.Domain.Common;
using POS.Domain.Repositories;

namespace POS.Application.Queries.Category.GetPaged;

public class GetCategorysPagedQueryHandler : IRequestHandler<GetCategorysPagedQuery, PagedResult<CategoryDto>>
{
    private readonly ICategoryRepository _repository;
    private readonly IMapper _mapper;

    public GetCategorysPagedQueryHandler(ICategoryRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PagedResult<CategoryDto>> Handle(GetCategorysPagedQuery request, CancellationToken cancellationToken)
    {
        var pagedEntities = await _repository.GetPagedAsync(request.PageNumber, request.PageSize);
        
        return new PagedResult<CategoryDto>
        {
            Items = _mapper.Map<IEnumerable<CategoryDto>>(pagedEntities.Items),
            TotalCount = pagedEntities.TotalCount,
            PageNumber = pagedEntities.PageNumber,
            PageSize = pagedEntities.PageSize
        };
    }
}
