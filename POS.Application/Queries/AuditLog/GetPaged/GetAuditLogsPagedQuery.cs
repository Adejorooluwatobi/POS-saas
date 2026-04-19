using MediatR;
using POS.Application.DTOs;
using POS.Domain.Common;

namespace POS.Application.Queries.AuditLog.GetPaged;

public record GetAuditLogsPagedQuery(Guid TenantId, int PageNumber, int PageSize) : IRequest<PagedResult<AuditLogDto>>;
