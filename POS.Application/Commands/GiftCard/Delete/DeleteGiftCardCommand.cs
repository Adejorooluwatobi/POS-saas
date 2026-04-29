using MediatR;

namespace POS.Application.Commands.GiftCard.Delete;

public record DeleteGiftCardCommand(Guid Id) : IRequest;
