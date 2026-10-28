using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using POS.Domain.Enums;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;

namespace POS.Application.Commands.Staff.GeneratePassword;

public class GenerateStaffPasswordCommandHandler : IRequestHandler<GenerateStaffPasswordCommand, GeneratedCredentialsDto>
{
    private readonly IStaffRepository _staffRepo;
    private readonly IPasswordService _passwordService;
    private readonly ITenantContext _tenantContext;
    private readonly IUnitOfWork _uow;

    public GenerateStaffPasswordCommandHandler(
        IStaffRepository staffRepo,
        IPasswordService passwordService,
        ITenantContext tenantContext,
        IUnitOfWork uow)
    {
        _staffRepo = staffRepo;
        _passwordService = passwordService;
        _tenantContext = tenantContext;
        _uow = uow;
    }

    public async Task<GeneratedCredentialsDto> Handle(GenerateStaffPasswordCommand request, CancellationToken cancellationToken)
    {
        var targetStaff = await _staffRepo.GetQueryable()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.Id == request.StaffId, cancellationToken)
            ?? throw new KeyNotFoundException($"Staff member {request.StaffId} not found.");

        var callerRole = _tenantContext.SystemRole;

        // ── STRICT HIERARCHICAL ACCESS CONTROL ─────────────────────────────
        if (callerRole == "SuperAdmin")
        {
            // Super Admin can reset only Tenant Admins
            if (targetStaff.SystemRole != SystemRole.TenantAdmin)
            {
                throw new UnauthorizedAccessException(
                    "Super Administrators can only reset passwords for Tenant Administrators. Store staff credentials must be managed by the organization's Tenant Admin or Managers.");
            }
        }
        else if (callerRole == "TenantAdmin")
        {
            // Tenant Admin can reset any staff in their tenant (except SuperAdmin)
            if (targetStaff.TenantId != _tenantContext.TenantId || targetStaff.SystemRole == SystemRole.SuperAdmin)
            {
                throw new UnauthorizedAccessException("Tenant Administrators can only manage staff within their own organization.");
            }
        }
        else if (callerRole == "Manager")
        {
            // General Manager can reset staff in their tenant (except SuperAdmin or TenantAdmin)
            if (targetStaff.TenantId != _tenantContext.TenantId ||
                targetStaff.SystemRole is SystemRole.SuperAdmin or SystemRole.TenantAdmin)
            {
                throw new UnauthorizedAccessException("General Managers cannot reset passwords for Tenant or System Administrators.");
            }
        }
        else if (callerRole == "StoreManager")
        {
            // Store Manager can ONLY reset staff assigned to their specific store branch
            if (targetStaff.TenantId != _tenantContext.TenantId ||
                !_tenantContext.StoreId.HasValue ||
                targetStaff.StoreId != _tenantContext.StoreId.Value ||
                targetStaff.SystemRole is SystemRole.SuperAdmin or SystemRole.TenantAdmin or SystemRole.Manager or SystemRole.StoreManager)
            {
                throw new UnauthorizedAccessException("Store Managers can only reset credentials for staff members assigned to their store branch.");
            }
        }
        else
        {
            throw new UnauthorizedAccessException("You do not have permission to reset staff credentials.");
        }

        // ── GENERATE TEMPORARY CREDENTIALS ──────────────────────────────
        var newPlainPassword = _passwordService.GenerateRandomPassword();
        targetStaff.PasswordHash = _passwordService.Hash(newPlainPassword);
        targetStaff.MustChangePassword = true;

        string? newPlainPin = null;
        if (targetStaff.SystemRole == SystemRole.Cashier)
        {
            newPlainPin = _passwordService.GenerateRandomPin();
            targetStaff.PinHash = _passwordService.Hash(newPlainPin);
            targetStaff.MustChangePin = true;
        }

        _staffRepo.Update(targetStaff);
        await _uow.SaveChangesAsync(cancellationToken);

        return new GeneratedCredentialsDto
        {
            StaffId = targetStaff.Id,
            Email = targetStaff.Email,
            FullName = targetStaff.FullName,
            GeneratedPassword = newPlainPassword,
            GeneratedPin = newPlainPin,
            SystemRole = targetStaff.SystemRole,
            Message = "Temporary credentials generated successfully. The user will be required to change them upon login."
        };
    }
}
