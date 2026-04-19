using MediatR;
using POS.Application.DTOs;
using POS.Domain.Common;

namespace POS.Application.Queries.TillSession.GetPaged;

public record GetTillSessionsPagedQuery(int PageNumber, int PageSize) : IRequest<PagedResult<TillSessionDto>>;
