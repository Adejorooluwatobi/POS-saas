using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using POS.Application.DTOs;
using POS.Domain.Common;
using POS.Domain.Repositories;

namespace POS.Application.Queries.Customer.GetPaged;

public class GetCustomersPagedQueryHandler : IRequestHandler<GetCustomersPagedQuery, PagedResult<CustomerDto>>
{
    private readonly ICustomerRepository _repository;
    private readonly IMapper _mapper;

    public GetCustomersPagedQueryHandler(ICustomerRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PagedResult<CustomerDto>> Handle(GetCustomersPagedQuery request, CancellationToken cancellationToken)
    {
        var pagedEntities = await _repository.GetPagedAsync(request.PageNumber, request.PageSize);
        
        return new PagedResult<CustomerDto>
        {
            Items = _mapper.Map<IEnumerable<CustomerDto>>(pagedEntities.Items),
            TotalCount = pagedEntities.TotalCount,
            PageNumber = pagedEntities.PageNumber,
            PageSize = pagedEntities.PageSize
        };
    }
}
