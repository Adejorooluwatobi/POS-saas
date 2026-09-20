using MediatR;
using POS.Application.DTOs;

namespace POS.Application.Commands.GiftCard.Recharge;

public record RechargeGiftCardCommand(RechargeGiftCardDto Dto) : IRequest<GiftCardDto>;
