using MediatR;
using POS.Application.DTOs;

namespace POS.Application.Commands.TenantSubscription.Update;

public record UpdateSubscriptionCommand(Guid TenantId, UpdateSubscriptionDto Dto) : IRequest<TenantSubscriptionDto>;
