using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using POS.Application.DTOs;
using POS.Domain.Enums;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;

namespace POS.Application.Commands.Terminal.Disconnect;

public class DisconnectTerminalCommandHandler : IRequestHandler<DisconnectTerminalCommand, TerminalDto>
{
    private readonly ITerminalRepository _repository;
    private readonly ITenantContext _tenantContext;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public DisconnectTerminalCommandHandler(
        ITerminalRepository repository,
        ITenantContext tenantContext,
        IUnitOfWork uow,
        IMapper mapper)
    {
        _repository = repository;
        _tenantContext = tenantContext;
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<TerminalDto> Handle(DisconnectTerminalCommand request, CancellationToken cancellationToken)
    {
        var terminal = await _repository.GetByIdAsync(request.TerminalId);
        if (terminal == null)
        {
            throw new InvalidOperationException("Terminal not found.");
        }

        // Tenancy Check
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
            throw new UnauthorizedAccessException("Branch managers can only disconnect terminals in their assigned store.");
        }

        terminal.Status = TerminalStatus.Disconnected;
        terminal.DeviceToken = null;
        terminal.LeaseExpiresAt = null;
        terminal.PairingCode = null;
        terminal.PairingCodeExpiresAt = null;
        terminal.DisconnectedAt = DateTimeOffset.UtcNow;
        terminal.DisconnectedBy = _tenantContext.UserName ?? "Admin";

        _repository.Update(terminal);
        await _uow.SaveChangesAsync(cancellationToken);

        return _mapper.Map<TerminalDto>(terminal);
    }
}
