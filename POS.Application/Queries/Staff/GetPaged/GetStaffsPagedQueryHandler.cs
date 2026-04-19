using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using POS.Application.DTOs;
using POS.Domain.Common;
using POS.Domain.Repositories;

namespace POS.Application.Queries.Staff.GetPaged;

public class GetStaffsPagedQueryHandler : IRequestHandler<GetStaffsPagedQuery, PagedResult<StaffDto>>
{
    private readonly IStaffRepository _repository;
    private readonly IMapper _mapper;

    public GetStaffsPagedQueryHandler(IStaffRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PagedResult<StaffDto>> Handle(GetStaffsPagedQuery request, CancellationToken cancellationToken)
    {
        var pagedEntities = await _repository.GetPagedAsync(request.PageNumber, request.PageSize);
        
        return new PagedResult<StaffDto>
        {
            Items = _mapper.Map<IEnumerable<StaffDto>>(pagedEntities.Items),
            TotalCount = pagedEntities.TotalCount,
            PageNumber = pagedEntities.PageNumber,
            PageSize = pagedEntities.PageSize
        };
    }
}
