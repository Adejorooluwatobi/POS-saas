using MediatR;
using POS.Application.DTOs;

namespace POS.Application.Commands.GiftCard.Update;

public record UpdateGiftCardCommand(Guid Id, UpdateGiftCardDto Dto) : IRequest;
