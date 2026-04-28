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
    private readonly IPasswordService _passwordService;
    private readonly ITokenService _tokenService;

    public LoginPosCommandHandler(
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

        var store = await _storeRepo.GetByIdAsync(request.Dto.StoreId);
        if (store != null && !store.IsActive)
        {
            throw new UnauthorizedAccessException("This store is currently suspended.");
        }

        var roleStr = staff.SystemRole.ToString();
        var token = _tokenService.GenerateToken(staff.Id, staff.EmployeeNo, roleStr, staff.FullName, staff.TenantId, staff.StoreId);

        return new AuthResponseDto(token, roleStr, staff.TenantId, staff.FullName);
    }
}
