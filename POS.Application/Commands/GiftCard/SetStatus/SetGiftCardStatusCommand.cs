using System;
using MediatR;
using POS.Application.DTOs;

namespace POS.Application.Commands.GiftCard.SetStatus;

public record SetGiftCardStatusCommand(Guid CardId, SetGiftCardStatusDto Dto) : IRequest<GiftCardDto>;
