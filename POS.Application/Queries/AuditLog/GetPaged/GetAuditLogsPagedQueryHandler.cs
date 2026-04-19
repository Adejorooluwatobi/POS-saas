using AutoMapper;
using MediatR;
using POS.Application.DTOs;
using POS.Domain.Common;
using POS.Domain.Repositories;

namespace POS.Application.Queries.AuditLog.GetPaged;

public class GetAuditLogsPagedQueryHandler : IRequestHandler<GetAuditLogsPagedQuery, PagedResult<AuditLogDto>>
{
    private readonly IAuditLogRepository _repository;
    private readonly IMapper _mapper;

    public GetAuditLogsPagedQueryHandler(IAuditLogRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PagedResult<AuditLogDto>> Handle(GetAuditLogsPagedQuery request, CancellationToken cancellationToken)
    {
        var paged = await _repository.GetPagedByTenantAsync(request.TenantId, request.PageNumber, request.PageSize);

        return new PagedResult<AuditLogDto>
        {
            Items = _mapper.Map<IEnumerable<AuditLogDto>>(paged.Items),
            TotalCount = paged.TotalCount,
            PageNumber = paged.PageNumber,
            PageSize = paged.PageSize
        };
    }
}
