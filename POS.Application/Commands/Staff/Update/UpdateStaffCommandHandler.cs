using AutoMapper;
using MediatR;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;

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

        // Store Scoped Permission Check
        if (_tenantContext.SystemRole == "Supervisor" || _tenantContext.SystemRole == "Manager")
        {
            if (entity.StoreId != _tenantContext.StoreId)
            {
                throw new UnauthorizedAccessException("You can only manage staff within your assigned store.");
            }

            // Prevent changing the StoreId to something else
            if (request.Dto.StoreId != _tenantContext.StoreId)
            {
                throw new UnauthorizedAccessException("You cannot reassign staff to other stores.");
            }
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
