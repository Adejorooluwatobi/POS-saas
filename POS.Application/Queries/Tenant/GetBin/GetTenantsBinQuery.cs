using MediatR;
using POS.Application.DTOs;
using POS.Domain.Common;

namespace POS.Application.Queries.Tenant.GetBin;

public record GetTenantsBinQuery(int PageNumber = 1, int PageSize = 50) : IRequest<PagedResult<TenantDto>>;
