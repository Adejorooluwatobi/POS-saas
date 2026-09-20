using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using POS.Application.DTOs;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;
using Entity = POS.Domain.Entities.Customer;

namespace POS.Application.Commands.Customer.Create;

public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, CustomerDto>
{
    private readonly ICustomerRepository _repository;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ITenantContext _tenantContext;
    private readonly IEncryptionService _encryptionService;
    private readonly ILoyaltyCardNumberGenerator _loyaltyGenerator;

    public CreateCustomerCommandHandler(
        ICustomerRepository repository,
        IUnitOfWork uow,
        IMapper mapper,
        ITenantContext tenantContext,
        IEncryptionService encryptionService,
        ILoyaltyCardNumberGenerator loyaltyGenerator)
    {
        _repository = repository;
        _uow = uow;
        _mapper = mapper;
        _tenantContext = tenantContext;
        _encryptionService = encryptionService;
        _loyaltyGenerator = loyaltyGenerator;
    }

    public async Task<CustomerDto> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var entity = _mapper.Map<Entity>(request.Dto);
        entity.TenantId = _tenantContext.TenantId!.Value;

        // Normalize email and phone so empty strings are stored as NULL (avoiding unique index collisions on empty string)
        entity.Email = string.IsNullOrWhiteSpace(request.Dto.Email) ? null : request.Dto.Email.Trim();
        entity.Phone = string.IsNullOrWhiteSpace(request.Dto.Phone) ? null : request.Dto.Phone.Trim();

        // Registration Origin & Store
        entity.IsSelfRegistered = request.Dto.IsSelfRegistered ?? false;
        entity.RegisteredStoreId = request.Dto.RegisteredStoreId ?? _tenantContext.StoreId;
        entity.RegisteredByStaffId = _tenantContext.UserId;

        if (!string.IsNullOrWhiteSpace(entity.Email))
        {
            var exists = await _repository.GetQueryable()
                .AnyAsync(c => c.TenantId == entity.TenantId && c.Email != null && c.Email.ToLower() == entity.Email.ToLower(), cancellationToken);
            if (exists)
            {
                throw new InvalidOperationException("A customer with this email address is already registered.");
            }
        }

        if (string.IsNullOrWhiteSpace(entity.LoyaltyCardNo))
        {
            entity.LoyaltyCardNo = await _loyaltyGenerator.GenerateLoyaltyCardNumberAsync(entity.TenantId, _tenantContext.StoreId, cancellationToken);
        }

        if (!string.IsNullOrWhiteSpace(request.Dto.IdentityNumber))
        {
            entity.EncryptedIdentityNumber = _encryptionService.Encrypt(request.Dto.IdentityNumber.Trim());
            entity.IdentityType = request.Dto.IdentityType;
        }

        if (request.Dto.LivenessVerified || !string.IsNullOrWhiteSpace(request.Dto.PhotoUrl))
        {
            entity.IsIdentityVerified = true;
            entity.LivenessVerifiedAt = DateTimeOffset.UtcNow;
            entity.LivenessAuditLog = request.Dto.LivenessAuditLog;
            entity.PhotoUrl = request.Dto.PhotoUrl;
        }

        await _repository.AddAsync(entity);
        await _uow.SaveChangesAsync(cancellationToken);
        return _mapper.Map<CustomerDto>(entity);
    }
}
