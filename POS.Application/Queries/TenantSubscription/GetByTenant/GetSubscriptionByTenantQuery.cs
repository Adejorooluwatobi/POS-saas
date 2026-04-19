using MediatR;
using POS.Application.DTOs;

namespace POS.Application.Queries.TenantSubscription.GetByTenant;

public record GetSubscriptionByTenantQuery(Guid TenantId) : IRequest<TenantSubscriptionDto?>;
