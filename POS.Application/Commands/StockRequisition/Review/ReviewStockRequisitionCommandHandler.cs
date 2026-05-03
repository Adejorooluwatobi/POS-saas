using MediatR;
using POS.Domain.Enums;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;

namespace POS.Application.Commands.StockRequisition.Review;

public class ReviewStockRequisitionCommandHandler : IRequestHandler<ReviewStockRequisitionCommand>
{
    private readonly IStockRequisitionRepository _repository;
    private readonly IUnitOfWork _uow;
    private readonly ITenantContext _tenantContext;

    public ReviewStockRequisitionCommandHandler(
        IStockRequisitionRepository repository,
        IUnitOfWork uow,
        ITenantContext tenantContext)
    {
        _repository = repository;
        _uow = uow;
        _tenantContext = tenantContext;
    }

    public async Task Handle(ReviewStockRequisitionCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id)
            ?? throw new KeyNotFoundException("Requisition not found.");

        if (entity.Status != RequisitionStatus.Pending)
            return; // Already reviewed or processed

        if (!_tenantContext.IsSuperAdmin && _tenantContext.SystemRole != "TenantAdmin" && _tenantContext.SystemRole != "Manager")
            throw new UnauthorizedAccessException("Only Generals can review requisitions.");

        entity.Status = RequisitionStatus.UnderReview;
        entity.ReviewedByStaffId = _tenantContext.UserId;
        entity.ReviewedAt = DateTimeOffset.UtcNow;

        await _uow.SaveChangesAsync(cancellationToken);
    }
}
