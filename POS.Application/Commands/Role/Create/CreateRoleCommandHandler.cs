using AutoMapper;
using MediatR;
using POS.Application.DTOs;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;
using Entity = POS.Domain.Entities.Role;

namespace POS.Application.Commands.Role.Create;

public class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, RoleDto>
{
    private readonly IRoleRepository _repository;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public CreateRoleCommandHandler(IRoleRepository repository, IUnitOfWork uow, IMapper mapper)
    {
        _repository = repository;
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<RoleDto> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        var entity = _mapper.Map<Entity>(request.Dto);
        await _repository.AddAsync(entity);
        await _uow.SaveChangesAsync(cancellationToken);
        return _mapper.Map<RoleDto>(entity);
    }
}
