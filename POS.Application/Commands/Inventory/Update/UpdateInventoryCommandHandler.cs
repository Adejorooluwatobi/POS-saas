using AutoMapper;
using MediatR;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;

namespace POS.Application.Commands.Inventory.Update;

public class UpdateInventoryCommandHandler : IRequestHandler<UpdateInventoryCommand>
{
    private readonly IInventoryRepository _repository;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ITenantContext _tenantContext;

    public UpdateInventoryCommandHandler(
        IInventoryRepository repository,
        IUnitOfWork uow,
        IMapper mapper,
        ITenantContext tenantContext)
    {
        _repository = repository;
        _uow = uow;
        _mapper = mapper;
        _tenantContext = tenantContext;
    }

    public async Task Handle(UpdateInventoryCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id)
            ?? throw new KeyNotFoundException($"Inventory {request.Id} not found.");

        var isGeneral = _tenantContext.IsSuperAdmin || _tenantContext.SystemRole == "TenantAdmin" || (_tenantContext.SystemRole == "Manager" && !_tenantContext.StoreId.HasValue);
        if (!isGeneral && _tenantContext.StoreId.HasValue && entity.StoreId != _tenantContext.StoreId.Value)
        {
            throw new UnauthorizedAccessException("You can only adjust inventory for your own store.");
        }

        _mapper.Map(request.Dto, entity);
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        _repository.Update(entity);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
