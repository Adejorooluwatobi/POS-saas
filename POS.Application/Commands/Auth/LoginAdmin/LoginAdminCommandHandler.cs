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
    private readonly ITenantRepository _tenantRepo;
    private readonly ITenantSubscriptionRepository _subscriptionRepo;
    private readonly IPasswordService _passwordService;
    private readonly ITokenService _tokenService;

    public LoginAdminCommandHandler(
        IStaffRepository staffRepo,
        IStoreRepository storeRepo,
        ITenantRepository tenantRepo,
        ITenantSubscriptionRepository subscriptionRepo,
        IPasswordService passwordService,
        ITokenService tokenService)
    {
        _staffRepo = staffRepo;
        _storeRepo = storeRepo;
        _tenantRepo = tenantRepo;
        _subscriptionRepo = subscriptionRepo;
        _passwordService = passwordService;
        _tokenService = tokenService;
    }

    public async Task<AuthResponseDto> Handle(LoginAdminCommand request, CancellationToken cancellationToken)
    {
        // 1. Normalize email to lowercase and look up staff member
        var email = request.Dto.Email?.Trim().ToLowerInvariant() ?? string.Empty;
        var staff = await _staffRepo.GetByEmailAsync(email);

        if (staff is null || staff.IsDeleted || string.IsNullOrEmpty(staff.PasswordHash) || !_passwordService.Verify(request.Dto.Password, staff.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        if (!staff.IsActive)
        {
            throw new UnauthorizedAccessException("Your account has been suspended. Please contact your administrator.");
        }

        // Check if tenant is active and not deleted
        POS.Domain.Entities.Tenant? tenant = null;
        if (staff.SystemRole != SystemRole.SuperAdmin)
        {
            tenant = await _tenantRepo.GetByIdAsync(staff.TenantId);
            if (tenant != null && (!tenant.IsActive || tenant.IsDeleted))
            {
                throw new UnauthorizedAccessException("Your organization has been suspended or deleted. Please contact support.");
            }

            var sub = await _subscriptionRepo.GetByTenantAsync(staff.TenantId);
            if (sub != null && sub.IsExpired)
            {
                throw new UnauthorizedAccessException("Your organization subscription or free trial has expired. Please contact support to renew your plan.");
            }
        }

        // Check if store is active (if staff is assigned to one)
        POS.Domain.Entities.Store? assignedStore = null;
        if (staff.StoreId.HasValue && staff.SystemRole != SystemRole.SuperAdmin)
        {
            assignedStore = await _storeRepo.GetByIdAsync(staff.StoreId.Value);
            if (assignedStore != null && !assignedStore.IsActive)
            {
                throw new UnauthorizedAccessException("Your assigned store is currently inactive/suspended.");
            }
        }

        // 2. Generate token
        var roleStr = staff.SystemRole.ToString();
        
        // SuperAdmins don't have a TenantId restriction in their token
        Guid? tokenTenantId = staff.SystemRole == SystemRole.SuperAdmin ? null : staff.TenantId;
        
        var token = _tokenService.GenerateToken(staff.Id, staff.Email, roleStr, staff.FullName, tokenTenantId, staff.StoreId, staff.MustChangePassword, staff.MustChangePin);

        var businessName = (staff.SystemRole == SystemRole.SuperAdmin) ? "RetailOS Admin" : tenant?.BusinessName;

        return new AuthResponseDto(
            token, 
            roleStr, 
            staff.TenantId, 
            staff.FullName, 
            staff.Id, 
            staff.Email, 
            staff.StoreId, 
            businessName,
            assignedStore?.Name,
            assignedStore?.Address,
            assignedStore?.City,
            assignedStore?.Phone,
            tenant?.ContactEmail,
            staff.MustChangePassword,
            staff.MustChangePin
        );
    }
}
