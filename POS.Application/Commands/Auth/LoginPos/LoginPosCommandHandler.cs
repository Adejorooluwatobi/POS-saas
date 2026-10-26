using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using POS.Application.DTOs.Auth;
using POS.Domain.Enums;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;

namespace POS.Application.Commands.Auth.LoginPos;

public class LoginPosCommandHandler : IRequestHandler<LoginPosCommand, AuthResponseDto>
{
    private readonly IStaffRepository _staffRepo;
    private readonly IStoreRepository _storeRepo;
    private readonly ITenantRepository _tenantRepo;
    private readonly ITenantSubscriptionRepository _subscriptionRepo;
    private readonly ITerminalRepository _terminalRepo;
    private readonly IPasswordService _passwordService;
    private readonly ITokenService _tokenService;

    public LoginPosCommandHandler(
        IStaffRepository staffRepo,
        IStoreRepository storeRepo,
        ITenantRepository tenantRepo,
        ITenantSubscriptionRepository subscriptionRepo,
        ITerminalRepository terminalRepo,
        IPasswordService passwordService,
        ITokenService tokenService)
    {
        _staffRepo = staffRepo;
        _storeRepo = storeRepo;
        _tenantRepo = tenantRepo;
        _subscriptionRepo = subscriptionRepo;
        _terminalRepo = terminalRepo;
        _passwordService = passwordService;
        _tokenService = tokenService;
    }

    public async Task<AuthResponseDto> Handle(LoginPosCommand request, CancellationToken cancellationToken)
    {
        var staff = await _staffRepo.GetByEmployeeNoAsync(request.Dto.StoreId, request.Dto.EmployeeNo);

        // We use Verify for PIN too since it's hashed for security
        if (staff is null || !_passwordService.Verify(request.Dto.Pin, staff.PinHash))
        {
            throw new UnauthorizedAccessException("Invalid POS credentials or unknown Store.");
        }

        if (!staff.IsActive)
        {
            throw new UnauthorizedAccessException("Your account has been suspended.");
        }

        // Check if tenant is active
        var tenant = await _tenantRepo.GetByIdAsync(staff.TenantId);
        if (tenant != null && !tenant.IsActive)
        {
            throw new UnauthorizedAccessException("Your organization has been suspended. Please contact support.");
        }

        // Check subscription
        var sub = await _subscriptionRepo.GetByTenantAsync(staff.TenantId);
        if (sub != null && sub.IsExpired)
        {
            throw new UnauthorizedAccessException("Your organization subscription or free trial has expired. POS terminal access is disabled.");
        }

        // Check terminal if provided
        if (request.Dto.TerminalId.HasValue)
        {
            var terminal = await _terminalRepo.GetByIdAsync(request.Dto.TerminalId.Value);
            if (terminal != null)
            {
                if (terminal.Status == TerminalStatus.Disconnected)
                {
                    throw new UnauthorizedAccessException("This workstation has been disconnected. Please enter a reconnect PIN to activate.");
                }
                if (terminal.LeaseExpiresAt.HasValue && terminal.LeaseExpiresAt.Value < DateTimeOffset.UtcNow)
                {
                    throw new UnauthorizedAccessException("This workstation's 30-day lease has expired. Please enter a new activation code.");
                }
                terminal.LastPingAt = DateTimeOffset.UtcNow;
                _terminalRepo.Update(terminal);
            }
        }

        var store = await _storeRepo.GetByIdAsync(request.Dto.StoreId);
        if (store != null && !store.IsActive)
        {
            throw new UnauthorizedAccessException("This store is currently suspended.");
        }

        var roleStr = staff.SystemRole.ToString();
        var token = _tokenService.GenerateToken(staff.Id, staff.EmployeeNo, roleStr, staff.FullName, staff.TenantId, staff.StoreId);

        return new AuthResponseDto(
            token, 
            roleStr, 
            staff.TenantId, 
            staff.FullName, 
            staff.Id, 
            staff.Email, 
            staff.StoreId, 
            tenant?.BusinessName,
            store?.Name,
            store?.Address,
            store?.City,
            store?.Phone,
            tenant?.ContactEmail
        );
    }
}
