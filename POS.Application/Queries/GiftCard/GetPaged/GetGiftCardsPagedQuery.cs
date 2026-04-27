using MediatR;
using POS.Application.DTOs;
using POS.Domain.Common;

namespace POS.Application.Queries.GiftCard.GetPaged;

public record GetGiftCardsPagedQuery(int PageNumber, int PageSize) : IRequest<PagedResult<GiftCardDto>>;
