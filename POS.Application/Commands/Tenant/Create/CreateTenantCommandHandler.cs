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

        // 2. Setup Admin Staff
        var adminPassword = _passwordService.Hash(request.Dto.AdminPassword);
        var adminStaff = new POS.Domain.Entities.Staff
        {
            TenantId = tenant.Id,
            SystemRole = POS.Domain.Enums.SystemRole.TenantAdmin,
            EmployeeNo = $"{request.Dto.Slug.ToUpper()}-ADM",
            Email = request.Dto.AdminEmail,
            FirstName = request.Dto.AdminFirstName,
            LastName = request.Dto.AdminLastName,
            PasswordHash = adminPassword,
            PinHash = adminPassword, // Shared for now
            HiredAt = DateOnly.FromDateTime(DateTime.Today),
            IsActive = true
        };

        await _repository.AddAsync(tenant);
        await _staffRepository.AddAsync(adminStaff);
        
        await _uow.SaveChangesAsync(cancellationToken);
        
        return _mapper.Map<TenantDto>(tenant);
    }
}
