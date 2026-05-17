using MediatR;
using POS.Application.DTOs;

namespace POS.Application.Queries.GiftCard.GetByNumber;

public record GetGiftCardByNumberQuery(string CardNumber) : IRequest<GiftCardDto?>;
