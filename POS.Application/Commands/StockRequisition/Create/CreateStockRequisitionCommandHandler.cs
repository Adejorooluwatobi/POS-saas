using AutoMapper;
using MediatR;
using POS.Application.DTOs.StockRequisition;
using POS.Domain.Entities;
using POS.Domain.Enums;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;

namespace POS.Application.Commands.StockRequisition.Create;

public class CreateStockRequisitionCommandHandler : IRequestHandler<CreateStockRequisitionCommand, StockRequisitionDto>
{
    private readonly IStockRequisitionRepository _repository;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ITenantContext _tenantContext;

    public CreateStockRequisitionCommandHandler(
        IStockRequisitionRepository repository,
        IUnitOfWork uow,
        IMapper mapper,
        ITenantContext tenantContext)
    {
        _repository = repository;
        _uow = uow;
        _mapper = mapper;
        _tenantContext = tenantContext;
    }

    public async Task<StockRequisitionDto> Handle(CreateStockRequisitionCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId ?? throw new UnauthorizedAccessException("Tenant context missing.");
        var staffId = _tenantContext.UserId ?? throw new UnauthorizedAccessException("User context missing.");
        var storeId = _tenantContext.StoreId ?? throw new InvalidOperationException("Requisitions must be created from a store context.");

        var requisitionNumber = $"REQ-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";

        var entity = new Domain.Entities.StockRequisition
        {
            TenantId = tenantId,
            RequisitionNumber = requisitionNumber,
            Status = RequisitionStatus.Pending,
            RequestingStoreId = storeId,
            CreatedByStaffId = staffId,
            Notes = request.Dto.Notes,
            Items = request.Dto.Items.Select(i => new StockRequisitionItem
            {
                StockRequisitionId = Guid.Empty,
                VariantId = i.VariantId,
                QuantityRequested = i.QuantityRequested
            }).ToList()
        };

        await _repository.AddAsync(entity);
        await _uow.SaveChangesAsync(cancellationToken);

        return _mapper.Map<StockRequisitionDto>(entity);
    }
}
