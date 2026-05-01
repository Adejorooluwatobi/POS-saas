using MediatR;
using POS.Application.DTOs;
using POS.Domain.Common;

namespace POS.Application.Queries.Terminal.GetAll;

public record GetAllTerminalsQuery(int PageNumber = 1, int PageSize = 20) : IRequest<PagedResult<TerminalDto>>;
