using MediatR;
using POS.Application.DTOs;

namespace POS.Application.Commands.GiftCard.Transfer;

public record TransferCardBalanceCommand(TransferCardBalanceDto Dto) : IRequest<GiftCardDto>;
