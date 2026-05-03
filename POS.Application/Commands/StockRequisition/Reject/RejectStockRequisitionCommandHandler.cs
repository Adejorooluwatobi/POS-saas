using MediatR;
using POS.Domain.Enums;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;

namespace POS.Application.Commands.StockRequisition.Reject;

public class RejectStockRequisitionCommandHandler : IRequestHandler<RejectStockRequisitionCommand>
{
    private readonly IStockRequisitionRepository _repository;
    private readonly IUnitOfWork _uow;
    private readonly ITenantContext _tenantContext;

    public RejectStockRequisitionCommandHandler(
        IStockRequisitionRepository repository,
        IUnitOfWork uow,
        ITenantContext tenantContext)
    {
        _repository = repository;
        _uow = uow;
        _tenantContext = tenantContext;
    }

    public async Task Handle(RejectStockRequisitionCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id)
            ?? throw new KeyNotFoundException("Requisition not found.");

        if (entity.Status != RequisitionStatus.Pending && entity.Status != RequisitionStatus.UnderReview)
            throw new InvalidOperationException("Only pending or under review requisitions can be rejected.");

        if (!_tenantContext.IsSuperAdmin && _tenantContext.SystemRole != "TenantAdmin" && _tenantContext.SystemRole != "Manager")
            throw new UnauthorizedAccessException("Only Generals can reject requisitions.");

        entity.Status = RequisitionStatus.Rejected;
        entity.RejectionReason = request.Reason;
        entity.ReviewedByStaffId = _tenantContext.UserId;
        entity.ReviewedAt = DateTimeOffset.UtcNow;

        await _uow.SaveChangesAsync(cancellationToken);

        // TODO: Send email to requesting StoreManager
    }
}
