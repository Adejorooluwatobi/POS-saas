using MediatR;
using POS.Application.DTOs;
using POS.Domain.Common;

namespace POS.Application.Queries.Staff.GetBin;

public record GetStaffBinQuery(int PageNumber = 1, int PageSize = 50) : IRequest<PagedResult<StaffDto>>;
