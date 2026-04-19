using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using POS.Application.DTOs.Auth;
using POS.Domain.Entities;
using POS.Domain.Enums;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;

namespace POS.Application.Commands.Auth.RegisterTenant;

public class RegisterTenantCommandHandler : IRequestHandler<RegisterTenantCommand, AuthResponseDto>
{
    private readonly ITenantRepository _tenantRepo;
    private readonly IStaffRepository _staffRepo;
    private readonly IPasswordService _passwordService;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _uow;

    public RegisterTenantCommandHandler(
        ITenantRepository tenantRepo,
        IStaffRepository staffRepo,
        IPasswordService passwordService,
        ITokenService tokenService,
        IUnitOfWork uow)
    {
        _tenantRepo = tenantRepo;
        _staffRepo = staffRepo;
        _passwordService = passwordService;
        _tokenService = tokenService;
        _uow = uow;
    }

    public async Task<AuthResponseDto> Handle(RegisterTenantCommand request, CancellationToken cancellationToken)
    {
        // 1. Create Tenant
        var tenantId = Guid.NewGuid();
        var tenant = new POS.Domain.Entities.Tenant
        {
            Id = tenantId,
            BusinessName = request.Dto.BusinessName,
            Slug = request.Dto.Slug,
            ContactEmail = request.Dto.ContactEmail,
            Country = request.Dto.Country,
            IsActive = true
        };

        await _tenantRepo.AddAsync(tenant);

        // 2. Create Owner Staff record
        var adminPassword = _passwordService.Hash(request.Dto.OwnerPassword);
        
        var adminStaff = new POS.Domain.Entities.Staff
        {
            TenantId = tenantId,
            FirstName = request.Dto.OwnerFirstName,
            LastName = request.Dto.OwnerLastName,
            Email = request.Dto.OwnerEmail,
            PasswordHash = adminPassword,
            PinHash = _passwordService.Hash("0000"), // Default Pin
            EmployeeNo = "ADMIN-01",
            SystemRole = SystemRole.TenantAdmin,
            HiredAt = DateOnly.FromDateTime(DateTime.UtcNow),
            IsActive = true
        };

        await _staffRepo.AddAsync(adminStaff);

        // 3. Save atomically
        await _uow.SaveChangesAsync(cancellationToken);

        // 4. Generate Auth Token
        var roleStr = adminStaff.SystemRole.ToString();
        var token = _tokenService.GenerateToken(adminStaff.Id, adminStaff.Email, roleStr, tenantId, null);

        return new AuthResponseDto(token, roleStr, tenantId, adminStaff.FullName);
    }
}
