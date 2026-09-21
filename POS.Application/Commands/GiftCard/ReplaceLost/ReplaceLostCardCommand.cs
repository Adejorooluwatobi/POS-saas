using MediatR;
using POS.Application.DTOs;

namespace POS.Application.Commands.GiftCard.ReplaceLost;

public record ReplaceLostCardCommand(ReplaceLostGiftCardDto Dto) : IRequest<GiftCardDto>;
