using MediatR;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;

namespace POS.Application.Commands.Coupon.Delete;

// Soft-deactivation: coupons may be referenced in completed transactions, so we never hard-delete.
public class DeleteCouponCommandHandler : IRequestHandler<DeleteCouponCommand>
{
    private readonly ICouponRepository _repository;
    private readonly IUnitOfWork _uow;

    public DeleteCouponCommandHandler(ICouponRepository repository, IUnitOfWork uow)
    {
        _repository = repository;
        _uow = uow;
    }

    public async Task Handle(DeleteCouponCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id)
            ?? throw new KeyNotFoundException($"Coupon {request.Id} not found.");

        entity.IsActive = false;
        _repository.Update(entity);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
