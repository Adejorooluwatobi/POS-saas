using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using POS.Application.DTOs;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;
using POS.Domain.Enums;
using Entity = POS.Domain.Entities.Staff;

namespace POS.Application.Commands.Staff.Create;

public class CreateStaffCommandHandler : IRequestHandler<CreateStaffCommand, StaffDto>
{
    private readonly IStaffRepository _repository;
    private readonly IRoleRepository _roleRepository;
    private readonly ITenantSubscriptionRepository _subscriptionRepository;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ITenantContext _tenantContext;
    private readonly IPasswordService _passwordService;

    public CreateStaffCommandHandler(
        IStaffRepository repository, 
        IRoleRepository roleRepository,
        ITenantSubscriptionRepository subscriptionRepository,
        IUnitOfWork uow, 
        IMapper mapper,
        ITenantContext tenantContext, 
        IPasswordService passwordService)
    {
        _repository = repository;
        _roleRepository = roleRepository;
        _subscriptionRepository = subscriptionRepository;
        _uow = uow;
        _mapper = mapper;
        _tenantContext = tenantContext;
        _passwordService = passwordService;
    }

    public async Task<StaffDto> Handle(CreateStaffCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId ?? throw new UnauthorizedAccessException("Unable to determine the tenant.");

        // ── Subscription Check ───────────────────────────────────────────
        var sub = await _subscriptionRepository.GetByTenantAsync(tenantId);
        if (sub != null)
        {
            var currentStaff = _repository.GetQueryable().Count(s => s.TenantId == tenantId && !s.IsDeleted);
            if (currentStaff >= sub.MaxStaff)
            {
                throw new InvalidOperationException($"Staff limit reached ({sub.MaxStaff}). Please upgrade your subscription.");
            }
        }

        var entity = _mapper.Map<Entity>(request.Dto);
        entity.TenantId = tenantId;
        entity.IsDeleted = false;
        entity.Email = entity.Email.Trim().ToLowerInvariant();

        // 1. Resolve SystemRole from RoleId if provided
        if (request.Dto.RoleId.HasValue)
        {
            var role = await _roleRepository.GetByIdAsync(request.Dto.RoleId.Value);
            if (role != null)
            {
                entity.SystemRole = role.SystemRole;
            }
        }
        else
        {
            entity.SystemRole = request.Dto.SystemRole;
        }

        // 2. Role Hierarchy Validation
        var creatorRole = _tenantContext.SystemRole;
        var targetRole = entity.SystemRole;

        if (creatorRole is "SuperAdmin" or "TenantAdmin")
        {
            // Admins can create any role
        }
        else if (creatorRole == "Manager")
        {
            // Manager can only create StoreManager, Cashier, Supervisor
            if (targetRole is SystemRole.SuperAdmin or SystemRole.TenantAdmin or SystemRole.Manager)
                throw new UnauthorizedAccessException("General Managers cannot create Admin or other Manager roles.");
        }
        else if (creatorRole == "StoreManager")
        {
            // StoreManager can only create Cashier or Supervisor
            if (targetRole is not (SystemRole.Cashier or SystemRole.Supervisor))
                throw new UnauthorizedAccessException("Store Managers can only create Cashiers or Supervisors.");
            
            // Force StoreId to match creator's
            entity.StoreId = _tenantContext.StoreId;
        }
        else if (creatorRole == "Supervisor")
        {
            // Supervisor can only create Cashier
            if (targetRole is not SystemRole.Cashier)
                throw new UnauthorizedAccessException("Supervisors can only create Cashiers.");
            
            // Force StoreId to match creator's
            entity.StoreId = _tenantContext.StoreId;
        }
        else
        {
            throw new UnauthorizedAccessException("You do not have permission to create staff.");
        }

        // 3. Auto-generate credentials
        var plainPassword = string.IsNullOrWhiteSpace(request.Dto.Password)
            ? _passwordService.GenerateRandomPassword()
            : request.Dto.Password;

        var plainPin = string.IsNullOrWhiteSpace(request.Dto.Pin)
            ? _passwordService.GenerateRandomPin()
            : request.Dto.Pin;

        entity.PasswordHash = _passwordService.Hash(plainPassword);
        entity.PinHash = _passwordService.Hash(plainPin);
        entity.MustChangePassword = true;

        // Cashiers are required to set their 4-digit PIN upon first login
        if (entity.SystemRole == SystemRole.Cashier)
        {
            entity.MustChangePin = true;
        }

        await _repository.AddAsync(entity);
        await _uow.SaveChangesAsync(cancellationToken);

        var dto = _mapper.Map<StaffDto>(entity);
        dto.GeneratedPassword = plainPassword;
        dto.GeneratedPin = plainPin;
        dto.HasPassword = true;
        dto.HasPin = true;
        dto.MustChangePassword = entity.MustChangePassword;
        dto.MustChangePin = entity.MustChangePin;
        return dto;
    }
}
