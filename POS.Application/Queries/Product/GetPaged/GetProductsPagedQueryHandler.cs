using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using POS.Application.DTOs;
using POS.Domain.Common;
using POS.Domain.Repositories;

namespace POS.Application.Queries.Product.GetPaged;

public class GetProductsPagedQueryHandler : IRequestHandler<GetProductsPagedQuery, PagedResult<ProductDto>>
{
    private readonly IProductRepository _repository;
    private readonly IMapper _mapper;

    public GetProductsPagedQueryHandler(IProductRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PagedResult<ProductDto>> Handle(GetProductsPagedQuery request, CancellationToken cancellationToken)
    {
        var pagedEntities = await _repository.GetPagedAsync(request.PageNumber, request.PageSize);
        
        return new PagedResult<ProductDto>
        {
            Items = _mapper.Map<IEnumerable<ProductDto>>(pagedEntities.Items),
            TotalCount = pagedEntities.TotalCount,
            PageNumber = pagedEntities.PageNumber,
            PageSize = pagedEntities.PageSize
        };
    }
}
