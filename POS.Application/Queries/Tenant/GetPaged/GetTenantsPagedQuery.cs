using MediatR;
using POS.Application.DTOs;
using POS.Domain.Common;

namespace POS.Application.Queries.Tenant.GetPaged;

public record GetTenantsPagedQuery(int PageNumber, int PageSize) : IRequest<PagedResult<TenantDto>>;
