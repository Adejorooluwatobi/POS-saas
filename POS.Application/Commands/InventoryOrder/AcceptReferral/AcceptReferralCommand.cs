using MediatR;

namespace POS.Application.Commands.InventoryOrder.AcceptReferral;

public record AcceptReferralCommand(Guid Id) : IRequest;
