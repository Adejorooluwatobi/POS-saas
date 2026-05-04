using AutoMapper;
using MediatR;
using POS.Application.DTOs;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;
using Entity = POS.Domain.Entities.Promotion;

namespace POS.Application.Commands.Promotion.Create;

public class CreatePromotionCommandHandler : IRequestHandler<CreatePromotionCommand, PromotionDto>
{
    private readonly IPromotionRepository _repository;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ITenantContext _tenantContext;

    public CreatePromotionCommandHandler(IPromotionRepository repository, IUnitOfWork uow, IMapper mapper, ITenantContext tenantContext)
    {
        _repository = repository;
        _uow = uow;
        _mapper = mapper;
        _tenantContext = tenantContext;
    }

    public async Task<PromotionDto> Handle(CreatePromotionCommand request, CancellationToken cancellationToken)
    {
        var entity = _mapper.Map<Entity>(request.Dto);
        entity.TenantId = _tenantContext.TenantId!.Value;

        // Scoping Logic: Inherited Redemption & Hierarchical Control
        if (_tenantContext.SystemRole is "StoreManager" or "Supervisor" or "Cashier")
        {
            // Store Managers and below are strictly restricted to their own store
            entity.StoreId = _tenantContext.StoreId;
        }
        else
        {
            // Admins (SuperAdmin, TenantAdmin, Manager) can create Global (null) or Store-Specific promos
            entity.StoreId = request.Dto.StoreId;
        }

        await _repository.AddAsync(entity);
        await _uow.SaveChangesAsync(cancellationToken);
        return _mapper.Map<PromotionDto>(entity);
    }
}
