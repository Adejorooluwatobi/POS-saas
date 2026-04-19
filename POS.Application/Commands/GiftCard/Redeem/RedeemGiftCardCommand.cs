using MediatR;
using POS.Application.DTOs;

namespace POS.Application.Commands.GiftCard.Redeem;

public record RedeemGiftCardCommand(RedeemGiftCardDto Dto) : IRequest<GiftCardDto>;
