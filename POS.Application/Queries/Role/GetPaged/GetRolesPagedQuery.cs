using MediatR;
using POS.Application.DTOs;
using POS.Domain.Common;

namespace POS.Application.Queries.Role.GetPaged;

public record GetRolesPagedQuery(int PageNumber, int PageSize) : IRequest<PagedResult<RoleDto>>;
