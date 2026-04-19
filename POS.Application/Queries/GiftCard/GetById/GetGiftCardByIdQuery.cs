using MediatR;
using POS.Application.DTOs;

namespace POS.Application.Queries.GiftCard.GetById;

public record GetGiftCardByIdQuery(Guid Id) : IRequest<GiftCardDto?>;
