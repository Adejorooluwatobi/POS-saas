using MediatR;
using POS.Application.DTOs;
using POS.Domain.Common;

namespace POS.Application.Queries.Staff.GetPaged;

public record GetStaffsPagedQuery(int PageNumber, int PageSize) : IRequest<PagedResult<StaffDto>>;
