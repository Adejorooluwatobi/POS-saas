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
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ITenantContext _tenantContext;
    private readonly IPasswordService _passwordService;

    public CreateStaffCommandHandler(
        IStaffRepository repository, IUnitOfWork uow, IMapper mapper,
        ITenantContext tenantContext, IPasswordService passwordService)
    {
        _repository = repository;
        _uow = uow;
        _mapper = mapper;
        _tenantContext = tenantContext;
        _passwordService = passwordService;
    }

    public async Task<StaffDto> Handle(CreateStaffCommand request, CancellationToken cancellationToken)
    {
        var entity = _mapper.Map<Entity>(request.Dto);

        // Role Hierarchy Validation
        var creatorRole = _tenantContext.SystemRole;
        var targetRole = request.Dto.SystemRole;

        if (creatorRole == "Manager")
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
        else if (creatorRole != "SuperAdmin" && creatorRole != "TenantAdmin")
        {
            throw new UnauthorizedAccessException("You do not have permission to create staff.");
        }

        entity.TenantId = _tenantContext.TenantId!.Value;
        entity.PinHash = _passwordService.Hash(request.Dto.Pin);

        if (!string.IsNullOrWhiteSpace(request.Dto.Password))
            entity.PasswordHash = _passwordService.Hash(request.Dto.Password);

        await _repository.AddAsync(entity);
        await _uow.SaveChangesAsync(cancellationToken);
        return _mapper.Map<StaffDto>(entity);
    }
}
