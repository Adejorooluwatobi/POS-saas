using AutoMapper;
using MediatR;
using POS.Application.DTOs;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;
using Entity = POS.Domain.Entities.Terminal;

namespace POS.Application.Commands.Terminal.Create;

public class CreateTerminalCommandHandler : IRequestHandler<CreateTerminalCommand, TerminalDto>
{
    private readonly ITerminalRepository _repository;
    private readonly ITenantSubscriptionRepository _subscriptionRepository;
    private readonly ITenantContext _tenantContext;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public CreateTerminalCommandHandler(
        ITerminalRepository repository, 
        ITenantSubscriptionRepository subscriptionRepository,
        ITenantContext tenantContext,
        IUnitOfWork uow, 
        IMapper mapper)
    {
        _repository = repository;
        _subscriptionRepository = subscriptionRepository;
        _tenantContext = tenantContext;
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<TerminalDto> Handle(CreateTerminalCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId ?? throw new UnauthorizedAccessException("Unable to determine the tenant.");

        // ── Subscription Check ───────────────────────────────────────────
        var sub = await _subscriptionRepository.GetByTenantAsync(tenantId);
        if (sub != null)
        {
            var currentTerminals = _repository.GetQueryable().Count(t => t.Store.TenantId == tenantId);
            if (currentTerminals >= sub.MaxTerminals)
            {
                throw new InvalidOperationException($"Terminal limit reached ({sub.MaxTerminals}). Please upgrade your subscription.");
            }
        }

        var entity = _mapper.Map<Entity>(request.Dto);
        entity.StoreId = request.StoreId;
        
        // Auto-generate terminal identity and pairing code
        entity.TerminalCode = $"TRM-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
        
        // Generate a 6-digit pairing code
        var random = new Random();
        entity.PairingCode = random.Next(100000, 999999).ToString();
        entity.PairingCodeExpiresAt = DateTimeOffset.UtcNow.AddHours(24);
        
        await _repository.AddAsync(entity);
        await _uow.SaveChangesAsync(cancellationToken);
        return _mapper.Map<TerminalDto>(entity);
    }
}
