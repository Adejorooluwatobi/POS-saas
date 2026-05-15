using AutoMapper;
using MediatR;
using POS.Application.DTOs;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;
using Entity = POS.Domain.Entities.Store;

namespace POS.Application.Commands.Store.Create;

public class CreateStoreCommandHandler : IRequestHandler<CreateStoreCommand, StoreDto>
{
    private readonly IStoreRepository _repository;
    private readonly ITenantSubscriptionRepository _subscriptionRepository;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ITenantContext _tenantContext;

    public CreateStoreCommandHandler(
        IStoreRepository repository, 
        ITenantSubscriptionRepository subscriptionRepository,
        IUnitOfWork uow, 
        IMapper mapper, 
        ITenantContext tenantContext)
    {
        _repository = repository;
        _subscriptionRepository = subscriptionRepository;
        _uow = uow;
        _mapper = mapper;
        _tenantContext = tenantContext;
    }

    public async Task<StoreDto> Handle(CreateStoreCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId!.Value;

        // ── Subscription Check ───────────────────────────────────────────
        var sub = await _subscriptionRepository.GetByTenantAsync(tenantId);
        if (sub != null)
        {
            var currentStores = _repository.GetQueryable().Count(s => s.TenantId == tenantId);
            if (currentStores >= sub.MaxStores)
            {
                throw new InvalidOperationException($"Store limit reached ({sub.MaxStores}). Please upgrade your subscription.");
            }
        }

        var entity = _mapper.Map<Entity>(request.Dto);
        entity.TenantId = tenantId;

        await _repository.AddAsync(entity);
        await _uow.SaveChangesAsync(cancellationToken);
        return _mapper.Map<StoreDto>(entity);
    }
}
