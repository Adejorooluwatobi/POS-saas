using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using POS.Application.DTOs.Auth;
using POS.Domain.Enums;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;

namespace POS.Application.Commands.Auth.LoginAdmin;

public class LoginAdminCommandHandler : IRequestHandler<LoginAdminCommand, AuthResponseDto>
{
    private readonly IStaffRepository _staffRepo;
    private readonly IStoreRepository _storeRepo;
    private readonly IPasswordService _passwordService;
    private readonly ITokenService _tokenService;

    public LoginAdminCommandHandler(
        IStaffRepository staffRepo,
        IStoreRepository storeRepo,
        IPasswordService passwordService,
        ITokenService tokenService)
    {
        _staffRepo = staffRepo;
        _storeRepo = storeRepo;
        _passwordService = passwordService;
        _tokenService = tokenService;
    }

    public async Task<AuthResponseDto> Handle(LoginAdminCommand request, CancellationToken cancellationToken)
    {
        // 1. We check if the email belongs to a staff member (who should be an admin/manager to use this endpoint)
        var staff = await _staffRepo.GetByEmailAsync(request.Dto.Email);

        if (staff is null || string.IsNullOrEmpty(staff.PasswordHash) || !_passwordService.Verify(request.Dto.Password, staff.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        if (!staff.IsActive)
        {
            throw new UnauthorizedAccessException("Your account has been suspended. Please contact your administrator.");
        }

        // Check if store is active (if staff is assigned to one)
        if (staff.StoreId.HasValue && staff.SystemRole != SystemRole.SuperAdmin)
        {
            var store = await _storeRepo.GetByIdAsync(staff.StoreId.Value);
            if (store != null && !store.IsActive)
            {
                throw new UnauthorizedAccessException("Your assigned store is currently inactive/suspended.");
            }
        }

        if (staff.SystemRole == SystemRole.Cashier)
        {
            throw new UnauthorizedAccessException("Cashiers cannot login to the dashboard. Please use the POS terminal.");
        }

        // 2. Generate token
        var roleStr = staff.SystemRole.ToString();
        
        // SuperAdmins don't have a TenantId restriction in their token
        Guid? tokenTenantId = staff.SystemRole == SystemRole.SuperAdmin ? null : staff.TenantId;
        
        var token = _tokenService.GenerateToken(staff.Id, staff.Email, roleStr, staff.FullName, tokenTenantId, staff.StoreId);

        return new AuthResponseDto(token, roleStr, staff.TenantId, staff.FullName);
    }
}
