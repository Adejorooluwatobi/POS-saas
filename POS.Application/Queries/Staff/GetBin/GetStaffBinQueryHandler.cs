using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using POS.Application.DTOs;
using POS.Domain.Common;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;

namespace POS.Application.Queries.Staff.GetBin;

public class GetStaffBinQueryHandler : IRequestHandler<GetStaffBinQuery, PagedResult<StaffDto>>
{
    private readonly IStaffRepository _repository;
    private readonly IMapper _mapper;
    private readonly ITenantContext _tenantContext;

    public GetStaffBinQueryHandler(IStaffRepository repository, IMapper mapper, ITenantContext tenantContext)
    {
        _repository = repository;
        _mapper = mapper;
        _tenantContext = tenantContext;
    }

    public async Task<PagedResult<StaffDto>> Handle(GetStaffBinQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;
        var query = _repository.GetQueryable()
            .IgnoreQueryFilters()
            .Where(s => s.IsDeleted);

        if (_tenantContext.SystemRole != "SuperAdmin")
        {
            if (!tenantId.HasValue)
                throw new UnauthorizedAccessException("Tenant context required.");

            query = query.Where(s => s.TenantId == tenantId.Value);

            if (_tenantContext.SystemRole is "StoreManager")
            {
                if (_tenantContext.StoreId.HasValue)
                {
                    query = query.Where(s => s.StoreId == _tenantContext.StoreId.Value);
                }
            }
        }

        query = query.Include(s => s.Store)
                     .Include(s => s.Role)
                     .OrderByDescending(s => s.DeletedAt ?? s.CreatedAt);

        var count = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = items.Select(s =>
        {
            var dto = _mapper.Map<StaffDto>(s);
            dto.StoreName = s.Store?.Name;
            dto.RoleName = s.Role?.Name;
            dto.HasPassword = !string.IsNullOrEmpty(s.PasswordHash);
            dto.HasPin = !string.IsNullOrEmpty(s.PinHash);
            return dto;
        }).ToList();

        return new PagedResult<StaffDto>
        {
            Items = dtos,
            TotalCount = count,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
