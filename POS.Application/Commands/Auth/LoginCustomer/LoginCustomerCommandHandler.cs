using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using POS.Application.DTOs.Auth;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;

namespace POS.Application.Commands.Auth.LoginCustomer;

public class LoginCustomerCommandHandler : IRequestHandler<LoginCustomerCommand, AuthResponseDto>
{
    private readonly ICustomerRepository _customerRepo;
    private readonly IPasswordService _passwordService;
    private readonly ITokenService _tokenService;

    public LoginCustomerCommandHandler(
        ICustomerRepository customerRepo,
        IPasswordService passwordService,
        ITokenService tokenService)
    {
        _customerRepo = customerRepo;
        _passwordService = passwordService;
        _tokenService = tokenService;
    }

    public async Task<AuthResponseDto> Handle(LoginCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _customerRepo.GetByEmailOrPhoneAsync(request.Dto.EmailOrPhone);

        if (customer is null || string.IsNullOrEmpty(customer.PasswordHash) || !_passwordService.Verify(request.Dto.Password, customer.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid customer credentials.");
        }

        // Generate token explicitly marked as Consumer
        var roleStr = "Consumer";
        var token = _tokenService.GenerateToken(customer.Id, customer.Email ?? customer.Phone ?? "unk", roleStr, customer.TenantId, null);

        var name = $"{customer.FirstName} {customer.LastName}".Trim();
        return new AuthResponseDto(token, roleStr, customer.TenantId, name);
    }
}
