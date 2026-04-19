using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using POS.Application.DTOs;
using POS.Domain.Common;
using POS.Domain.Repositories;

namespace POS.Application.Queries.Role.GetPaged;

public class GetRolesPagedQueryHandler : IRequestHandler<GetRolesPagedQuery, PagedResult<RoleDto>>
{
    private readonly IRoleRepository _repository;
    private readonly IMapper _mapper;

    public GetRolesPagedQueryHandler(IRoleRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PagedResult<RoleDto>> Handle(GetRolesPagedQuery request, CancellationToken cancellationToken)
    {
        var pagedEntities = await _repository.GetPagedAsync(request.PageNumber, request.PageSize);
        
        return new PagedResult<RoleDto>
        {
            Items = _mapper.Map<IEnumerable<RoleDto>>(pagedEntities.Items),
            TotalCount = pagedEntities.TotalCount,
            PageNumber = pagedEntities.PageNumber,
            PageSize = pagedEntities.PageSize
        };
    }
}
