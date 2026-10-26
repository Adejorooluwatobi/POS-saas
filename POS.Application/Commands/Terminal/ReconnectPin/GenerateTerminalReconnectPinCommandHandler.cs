using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using POS.Application.DTOs;
using POS.Domain.Enums;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;

namespace POS.Application.Commands.Terminal.ReconnectPin;

public class GenerateTerminalReconnectPinCommandHandler : IRequestHandler<GenerateTerminalReconnectPinCommand, ReconnectPinResponse>
{
    private readonly ITerminalRepository _repository;
    private readonly ITenantSubscriptionRepository _subscriptionRepository;
    private readonly ITenantContext _tenantContext;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public GenerateTerminalReconnectPinCommandHandler(
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

    public async Task<ReconnectPinResponse> Handle(GenerateTerminalReconnectPinCommand request, CancellationToken cancellationToken)
    {
        var terminal = await _repository.GetByIdAsync(request.TerminalId);
        if (terminal == null)
        {
            throw new InvalidOperationException("Terminal not found.");
        }

        // Tenancy Check
        var tenantId = terminal.Store?.TenantId ?? _tenantContext.TenantId;
        if (_tenantContext.SystemRole != "SuperAdmin" && _tenantContext.TenantId.HasValue && terminal.Store != null)
        {
            if (terminal.Store.TenantId != _tenantContext.TenantId.Value)
            {
                throw new UnauthorizedAccessException("You do not have permission to manage this terminal.");
            }
        }

        // Branch Manager Scope Check
        if (_tenantContext.SystemRole == "StoreManager" && _tenantContext.StoreId.HasValue && _tenantContext.StoreId.Value != terminal.StoreId)
        {
            throw new UnauthorizedAccessException("Branch managers can only generate reconnect PINs for terminals in their assigned store.");
        }

        // Subscription & Quota Check
        if (tenantId.HasValue)
        {
            var sub = await _subscriptionRepository.GetByTenantAsync(tenantId.Value);
            if (sub != null)
            {
                if (sub.IsExpired)
                {
                    throw new InvalidOperationException("Organization subscription has expired. Please contact support to renew.");
                }

                var activeOtherTerminals = _repository.GetQueryable().Count(t => 
                    t.Store.TenantId == tenantId.Value && 
                    t.Id != terminal.Id &&
                    t.Status != TerminalStatus.Disconnected &&
                    t.DeviceToken != null &&
                    (t.LeaseExpiresAt == null || t.LeaseExpiresAt > DateTimeOffset.UtcNow));

                if (activeOtherTerminals >= sub.MaxTerminals)
                {
                    throw new InvalidOperationException($"Active terminal limit reached ({sub.MaxTerminals}). Please disconnect another terminal before reconnecting this one.");
                }
            }
        }

        // Generate 6-digit numeric PIN
        var random = new Random();
        var pin = random.Next(100000, 999999).ToString();
        var expiresAt = DateTimeOffset.UtcNow.AddHours(48);

        terminal.PairingCode = pin;
        terminal.PairingCodeExpiresAt = expiresAt;

        _repository.Update(terminal);
        await _uow.SaveChangesAsync(cancellationToken);

        return new ReconnectPinResponse(pin, expiresAt, _mapper.Map<TerminalDto>(terminal));
    }
}
