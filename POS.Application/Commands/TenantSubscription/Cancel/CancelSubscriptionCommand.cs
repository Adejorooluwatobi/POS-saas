using MediatR;

namespace POS.Application.Commands.TenantSubscription.Cancel;

public record CancelSubscriptionCommand(Guid TenantId) : IRequest;
