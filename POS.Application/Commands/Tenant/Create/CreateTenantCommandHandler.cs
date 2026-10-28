using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using POS.Application.DTOs;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;
using Entity = POS.Domain.Entities.Tenant;

namespace POS.Application.Commands.Tenant.Create;

public class CreateTenantCommandHandler : IRequestHandler<CreateTenantCommand, TenantDto>
{
    private readonly ITenantRepository _repository;
    private readonly IStaffRepository _staffRepository;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly IPasswordService _passwordService;

    public CreateTenantCommandHandler(
        ITenantRepository repository, 
        IStaffRepository staffRepository,
        IUnitOfWork uow, 
        IMapper mapper,
        IPasswordService passwordService)
    {
        _repository = repository;
        _staffRepository = staffRepository;
        _uow = uow;
        _mapper = mapper;
        _passwordService = passwordService;
    }

    public async Task<TenantDto> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = _mapper.Map<Entity>(request.Dto);
        tenant.IsDeleted = false;
        if (!string.IsNullOrWhiteSpace(tenant.ContactEmail))
        {
            tenant.ContactEmail = tenant.ContactEmail.Trim().ToLowerInvariant();
        }
        
        // 1. Setup Subscription
        var now = DateTimeOffset.UtcNow;
        var periodEnd = request.Dto.DurationType switch
        {
            "14_DAYS" => now.AddDays(14),
            "30_DAYS" => now.AddDays(30),
            "60_DAYS" => now.AddDays(60),
            "90_DAYS" => now.AddDays(90),
            "1_YEAR"  => now.AddYears(1),
            "CUSTOM" when request.Dto.CustomExpiryDate.HasValue => request.Dto.CustomExpiryDate.Value,
            _ => now.AddDays(30)
        };

        var status = request.Dto.IsTrial 
            ? POS.Domain.Enums.SubscriptionStatus.Trial 
            : POS.Domain.Enums.SubscriptionStatus.Active;

        tenant.Subscription = new POS.Domain.Entities.TenantSubscription
        {
            TenantId = tenant.Id,
            MaxStores = request.Dto.MaxStores,
            MaxStaff = request.Dto.MaxStaff,
            MaxTerminals = request.Dto.MaxTerminals,
            Plan = request.Dto.Plan,
            Status = status,
            CurrentPeriodStart = now,
            CurrentPeriodEnd = periodEnd,
            TrialEndsAt = request.Dto.IsTrial ? periodEnd : null
        };

        // 2. Setup Admin Staff & Credentials
        var plainPassword = string.IsNullOrWhiteSpace(request.Dto.AdminPassword)
            ? _passwordService.GenerateRandomPassword()
            : request.Dto.AdminPassword;

        var plainPin = _passwordService.GenerateRandomPin();
        var adminPasswordHash = _passwordService.Hash(plainPassword);
        var adminPinHash = _passwordService.Hash(plainPin);

        var adminStaff = new POS.Domain.Entities.Staff
        {
            TenantId = tenant.Id,
            SystemRole = POS.Domain.Enums.SystemRole.TenantAdmin,
            EmployeeNo = $"{request.Dto.Slug.ToUpper()}-ADM",
            Email = request.Dto.AdminEmail.Trim().ToLowerInvariant(),
            FirstName = request.Dto.AdminFirstName,
            LastName = request.Dto.AdminLastName,
            PasswordHash = adminPasswordHash,
            PinHash = adminPinHash,
            HiredAt = DateOnly.FromDateTime(DateTime.Today),
            IsActive = true,
            IsDeleted = false,
            MustChangePassword = true,
            MustChangePin = false
        };

        await _repository.AddAsync(tenant);
        await _staffRepository.AddAsync(adminStaff);
        
        await _uow.SaveChangesAsync(cancellationToken);
        
        var dto = _mapper.Map<TenantDto>(tenant);
        dto.GeneratedPassword = plainPassword;
        dto.OwnerEmail = adminStaff.Email;
        dto.OwnerName = adminStaff.FullName;
        dto.OwnerStaffId = adminStaff.Id;
        return dto;
    }
}
