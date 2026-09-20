using MediatR;
using POS.Application.DTOs;

namespace POS.Application.Commands.GiftCard.LinkCustomer;

public record LinkGiftCardCustomerCommand(Guid GiftCardId, Guid CustomerId) : IRequest<GiftCardDto>;
public record UnlinkGiftCardCustomerCommand(Guid GiftCardId) : IRequest<GiftCardDto>;
