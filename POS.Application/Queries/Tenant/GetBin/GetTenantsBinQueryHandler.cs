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

namespace POS.Application.Queries.Tenant.GetBin;

public class GetTenantsBinQueryHandler : IRequestHandler<GetTenantsBinQuery, PagedResult<TenantDto>>
{
    private readonly ITenantRepository _repository;
    private readonly IMapper _mapper;

    public GetTenantsBinQueryHandler(ITenantRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PagedResult<TenantDto>> Handle(GetTenantsBinQuery request, CancellationToken cancellationToken)
    {
        var query = _repository.GetQueryable()
            .IgnoreQueryFilters()
            .Where(t => t.IsDeleted)
            .Include(t => t.Staff)
            .OrderByDescending(t => t.DeletedAt ?? t.UpdatedAt);

        var count = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = items.Select(t =>
        {
            var admin = t.Staff.FirstOrDefault(s => s.SystemRole == Domain.Enums.SystemRole.TenantAdmin)
                     ?? t.Staff.FirstOrDefault();
            var dto = _mapper.Map<TenantDto>(t);
            dto.OwnerEmail = admin?.Email;
            dto.OwnerName = admin?.FullName;
            dto.OwnerStaffId = admin?.Id;
            return dto;
        }).ToList();

        return new PagedResult<TenantDto>
        {
            Items = dtos,
            TotalCount = count,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
