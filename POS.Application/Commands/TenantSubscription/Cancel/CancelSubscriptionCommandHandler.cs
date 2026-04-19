using MediatR;
using POS.Domain.Enums;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;

namespace POS.Application.Commands.TenantSubscription.Cancel;

public class CancelSubscriptionCommandHandler : IRequestHandler<CancelSubscriptionCommand>
{
    private readonly ITenantSubscriptionRepository _repository;
    private readonly IUnitOfWork _uow;

    public CancelSubscriptionCommandHandler(ITenantSubscriptionRepository repository, IUnitOfWork uow)
    {
        _repository = repository;
        _uow = uow;
    }

    public async Task Handle(CancelSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByTenantAsync(request.TenantId)
            ?? throw new KeyNotFoundException($"No subscription found for tenant {request.TenantId}.");

        entity.Status = SubscriptionStatus.Cancelled;
        entity.CancelledAt = DateTimeOffset.UtcNow;
        _repository.Update(entity);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
