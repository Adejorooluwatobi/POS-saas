using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using POS.Domain.Enums;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;

namespace POS.Application.Commands.Auth.ChangePassword;

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, ChangePasswordResultDto>
{
    private readonly IStaffRepository _staffRepo;
    private readonly IPasswordService _passwordService;
    private readonly ITenantContext _tenantContext;
    private readonly IUnitOfWork _uow;

    public ChangePasswordCommandHandler(
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

    public async Task<ChangePasswordResultDto> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var userId = _tenantContext.UserId;
        if (!userId.HasValue)
        {
            throw new UnauthorizedAccessException("User identification not found in authentication context.");
        }

        var staff = await _staffRepo.GetQueryable()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.Id == userId.Value, cancellationToken)
            ?? throw new KeyNotFoundException("User account record not found.");

        if (string.IsNullOrWhiteSpace(request.Dto.CurrentPassword))
        {
            throw new ArgumentException("Current password is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Dto.NewPassword) || request.Dto.NewPassword.Length < 6)
        {
            throw new ArgumentException("New password must be at least 6 characters long.");
        }

        if (string.IsNullOrEmpty(staff.PasswordHash) || !_passwordService.Verify(request.Dto.CurrentPassword, staff.PasswordHash))
        {
            throw new UnauthorizedAccessException("Current password is incorrect.");
        }

        if (request.Dto.CurrentPassword == request.Dto.NewPassword)
        {
            throw new ArgumentException("New password cannot be the same as your current temporary password.");
        }

        // Cashier 4-digit PIN setup enforcement
        var requiresPin = staff.MustChangePin || (staff.SystemRole == SystemRole.Cashier && staff.MustChangePassword);
        if (requiresPin || !string.IsNullOrWhiteSpace(request.Dto.NewPin))
        {
            if (string.IsNullOrWhiteSpace(request.Dto.NewPin) || request.Dto.NewPin.Length != 4 || !int.TryParse(request.Dto.NewPin, out _))
            {
                throw new ArgumentException("Cashiers must configure a valid 4-digit numeric POS register PIN.");
            }

            staff.PinHash = _passwordService.Hash(request.Dto.NewPin);
            staff.MustChangePin = false;
        }

        staff.PasswordHash = _passwordService.Hash(request.Dto.NewPassword);
        staff.MustChangePassword = false;

        _staffRepo.Update(staff);
        await _uow.SaveChangesAsync(cancellationToken);

        return new ChangePasswordResultDto
        {
            Success = true,
            Message = "Your credentials have been updated successfully."
        };
    }
}
