using MediatR;
using POS.Application.DTOs;
using POS.Domain.Common;

namespace POS.Application.Queries.Promotion.GetPaged;

public record GetPromotionsPagedQuery(int PageNumber, int PageSize) : IRequest<PagedResult<PromotionDto>>;
