using AutoMapper;
using MediatR;
using POS.Application.DTOs;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;
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
        entity.TenantId = _tenantContext.TenantId!.Value;
        entity.PinHash = _passwordService.Hash(request.Dto.Pin);

        if (!string.IsNullOrWhiteSpace(request.Dto.Password))
            entity.PasswordHash = _passwordService.Hash(request.Dto.Password);

        await _repository.AddAsync(entity);
        await _uow.SaveChangesAsync(cancellationToken);
        return _mapper.Map<StaffDto>(entity);
    }
}
