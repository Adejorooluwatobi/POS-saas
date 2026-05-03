using MediatR;
using POS.Domain.Enums;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;

namespace POS.Application.Commands.StockRequisition.Cancel;

public class CancelStockRequisitionCommandHandler : IRequestHandler<CancelStockRequisitionCommand>
{
    private readonly IStockRequisitionRepository _repository;
    private readonly IUnitOfWork _uow;
    private readonly ITenantContext _tenantContext;

    public CancelStockRequisitionCommandHandler(
        IStockRequisitionRepository repository,
        IUnitOfWork uow,
        ITenantContext tenantContext)
    {
        _repository = repository;
        _uow = uow;
        _tenantContext = tenantContext;
    }

    public async Task Handle(CancelStockRequisitionCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id)
            ?? throw new KeyNotFoundException("Requisition not found.");

        if (entity.Status != RequisitionStatus.Pending)
            throw new InvalidOperationException("Only pending requisitions can be cancelled by the store.");

        if (_tenantContext.StoreId.HasValue && entity.RequestingStoreId != _tenantContext.StoreId.Value)
            throw new UnauthorizedAccessException("You can only cancel requisitions for your own store.");

        entity.Status = RequisitionStatus.Cancelled;

        await _uow.SaveChangesAsync(cancellationToken);
    }
}
