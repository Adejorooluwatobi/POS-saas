using MediatR;

namespace POS.Application.Commands.InventoryOrder.DeclineReferral;

public record DeclineReferralCommand(Guid Id, string Reason) : IRequest;
