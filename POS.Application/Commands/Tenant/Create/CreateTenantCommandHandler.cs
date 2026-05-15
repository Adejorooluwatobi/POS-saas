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
        tenant.Subscription = new POS.Domain.Entities.TenantSubscription
        {
            TenantId = tenant.Id,
            MaxStores = request.Dto.MaxStores,
            MaxStaff = request.Dto.MaxStaff,
            MaxTerminals = request.Dto.MaxTerminals,
            Plan = POS.Domain.Enums.SubscriptionPlan.Starter,
            Status = POS.Domain.Enums.SubscriptionStatus.Active,
            CurrentPeriodStart = DateTimeOffset.UtcNow,
            CurrentPeriodEnd = DateTimeOffset.UtcNow.AddYears(1) // Default to 1 year for manual onboard
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
