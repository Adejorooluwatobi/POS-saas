using MediatR;
using POS.Application.DTOs;

namespace POS.Application.Commands.GiftCard.Issue;

public record IssueGiftCardCommand(IssueGiftCardDto Dto) : IRequest<GiftCardDto>;
