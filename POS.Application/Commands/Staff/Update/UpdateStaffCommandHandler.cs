using AutoMapper;
using MediatR;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;
using POS.Domain.Enums;

namespace POS.Application.Commands.Staff.Update;

public class UpdateStaffCommandHandler : IRequestHandler<UpdateStaffCommand>
{
    private readonly IStaffRepository _repository;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly IPasswordService _passwordService;
    private readonly ITenantContext _tenantContext;

    public UpdateStaffCommandHandler(
        IStaffRepository repository, 
        IUnitOfWork uow, 
        IMapper mapper, 
        IPasswordService passwordService,
        ITenantContext tenantContext)
    {
        _repository = repository;
        _uow = uow;
        _mapper = mapper;
        _passwordService = passwordService;
        _tenantContext = tenantContext;
    }

    public async Task Handle(UpdateStaffCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id)
            ?? throw new KeyNotFoundException($"Staff {request.Id} not found.");

        // Hierarchical Role Validation
        var creatorRole = _tenantContext.SystemRole;
        var targetRole = request.Dto.SystemRole;

        if (creatorRole == "Manager")
        {
            // General Managers cannot modify Admins or other General Managers
            if (targetRole is SystemRole.SuperAdmin or SystemRole.TenantAdmin or SystemRole.Manager || 
                entity.SystemRole is SystemRole.SuperAdmin or SystemRole.TenantAdmin or SystemRole.Manager)
                throw new UnauthorizedAccessException("General Managers cannot manage Admin or other General Manager roles.");
        }
        else if (creatorRole == "StoreManager")
        {
            // Store Managers can only modify staff assigned to their store
            if (entity.StoreId != _tenantContext.StoreId)
                throw new UnauthorizedAccessException("You can only manage staff within your assigned store.");

            // Store Managers can only modify/set roles to Supervisor or Cashier
            if (targetRole is SystemRole.SuperAdmin or SystemRole.TenantAdmin or SystemRole.StoreManager or SystemRole.Manager || 
                entity.SystemRole is SystemRole.SuperAdmin or SystemRole.TenantAdmin or SystemRole.StoreManager or SystemRole.Manager)
                throw new UnauthorizedAccessException("Store Managers can only manage Cashiers or Supervisors.");

            // Prevent reassigning staff to other stores
            if (request.Dto.StoreId != _tenantContext.StoreId)
                throw new UnauthorizedAccessException("You cannot reassign staff to other stores.");
        }
        else if (creatorRole == "Supervisor")
        {
            throw new UnauthorizedAccessException("Supervisors do not have permission to edit staff details.");
        }

        _mapper.Map(request.Dto, entity);

        if (!string.IsNullOrWhiteSpace(request.Dto.Pin))
        {
            entity.PinHash = _passwordService.Hash(request.Dto.Pin);
        }

        if (request.Dto.Password != null)
        {
            if (string.IsNullOrWhiteSpace(request.Dto.Password))
            {
                entity.PasswordHash = null;
            }
            else
            {
                entity.PasswordHash = _passwordService.Hash(request.Dto.Password);
            }
        }
        _repository.Update(entity);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
