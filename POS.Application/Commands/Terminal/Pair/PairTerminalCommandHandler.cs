using AutoMapper;
using MediatR;
using POS.Application.DTOs;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;

namespace POS.Application.Commands.Terminal.Pair;

public class PairTerminalCommandHandler : IRequestHandler<PairTerminalCommand, PairTerminalResponse>
{
    private readonly ITerminalRepository _repository;
    private readonly ITenantSubscriptionRepository _subscriptionRepository;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public PairTerminalCommandHandler(
        ITerminalRepository repository, 
        ITenantSubscriptionRepository subscriptionRepository,
        IUnitOfWork uow, 
        IMapper mapper)
    {
        _repository = repository;
        _subscriptionRepository = subscriptionRepository;
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<PairTerminalResponse> Handle(PairTerminalCommand request, CancellationToken cancellationToken)
    {
        var terminal = await _repository.GetByPairingCodeAsync(request.PairingCode);

        if (terminal == null)
        {
            return new PairTerminalResponse { Success = false, Message = "Invalid pairing code." };
        }

        if (terminal.PairingCodeExpiresAt.HasValue && terminal.PairingCodeExpiresAt.Value < DateTimeOffset.UtcNow)
        {
            return new PairTerminalResponse { Success = false, Message = "Pairing code has expired. Please generate a new one in the Admin Portal." };
        }

        var tenantId = terminal.Store?.TenantId;
        if (tenantId.HasValue)
        {
            var sub = await _subscriptionRepository.GetByTenantAsync(tenantId.Value);
            if (sub != null)
            {
                if (sub.IsExpired)
                {
                    return new PairTerminalResponse { Success = false, Message = "Organization subscription has expired. Please contact support to renew." };
                }

                var activeOtherTerminals = _repository.GetQueryable().Count(t => 
                    t.Store.TenantId == tenantId.Value && 
                    t.Id != terminal.Id &&
                    t.Status != Domain.Enums.TerminalStatus.Disconnected &&
                    t.DeviceToken != null &&
                    (t.LeaseExpiresAt == null || t.LeaseExpiresAt > DateTimeOffset.UtcNow));

                if (activeOtherTerminals >= sub.MaxTerminals)
                {
                    return new PairTerminalResponse { Success = false, Message = $"Terminal limit reached ({sub.MaxTerminals}). Please disconnect an unused terminal in the Admin Portal." };
                }
            }
        }

        // Generate a fresh device token and 30-day lease
        var deviceToken = Guid.NewGuid().ToString("N");
        
        terminal.DeviceToken = deviceToken;
        terminal.PairingCode = null; // Clear pairing code
        terminal.PairingCodeExpiresAt = null;
        terminal.Status = Domain.Enums.TerminalStatus.Online;
        terminal.LeaseExpiresAt = DateTimeOffset.UtcNow.AddDays(30);
        terminal.DisconnectedAt = null;
        terminal.DisconnectedBy = null;
        terminal.LastPingAt = DateTimeOffset.UtcNow;

        _repository.Update(terminal);
        await _uow.SaveChangesAsync(cancellationToken);

        return new PairTerminalResponse
        {
            Success = true,
            Message = "Device successfully paired.",
            DeviceToken = deviceToken,
            Terminal = _mapper.Map<TerminalDto>(terminal)
        };
    }
}
