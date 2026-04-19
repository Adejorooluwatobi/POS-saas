using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using POS.Application.DTOs.Auth;
using POS.Domain.Entities;
using POS.Domain.Enums;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;

namespace POS.Application.Commands.Auth.RegisterCustomer;

public class RegisterCustomerCommandHandler : IRequestHandler<RegisterCustomerCommand, AuthResponseDto>
{
    private readonly ICustomerRepository _customerRepo;
    private readonly IPasswordService _passwordService;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _uow;

    public RegisterCustomerCommandHandler(
        ICustomerRepository customerRepo,
        IPasswordService passwordService,
        ITokenService tokenService,
        IUnitOfWork uow)
    {
        _customerRepo = customerRepo;
        _passwordService = passwordService;
        _tokenService = tokenService;
        _uow = uow;
    }

    public async Task<AuthResponseDto> Handle(RegisterCustomerCommand request, CancellationToken cancellationToken)
    {
        var existing = await _customerRepo.GetByEmailOrPhoneAsync(request.Dto.Email);
        if (existing is not null)
        {
            throw new InvalidOperationException("Email is already registered.");
        }

        var customer = new POS.Domain.Entities.Customer
        {
            TenantId = request.Dto.TenantId,
            FirstName = request.Dto.FirstName,
            LastName = request.Dto.LastName,
            Email = request.Dto.Email,
            Phone = request.Dto.Phone,
            PasswordHash = _passwordService.Hash(request.Dto.Password),
            IsActive = true,
            Tier = CustomerTier.Bronze
        };

        await _customerRepo.AddAsync(customer);
        await _uow.SaveChangesAsync(cancellationToken);

        var roleStr = "Consumer";
        var token = _tokenService.GenerateToken(customer.Id, customer.Email, roleStr, customer.TenantId, null);

        var name = $"{customer.FirstName} {customer.LastName}".Trim();
        return new AuthResponseDto(token, roleStr, customer.TenantId, name);
    }
}
