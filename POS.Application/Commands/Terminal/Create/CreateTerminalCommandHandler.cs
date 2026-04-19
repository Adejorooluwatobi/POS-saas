using AutoMapper;
using MediatR;
using POS.Application.DTOs;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;
using Entity = POS.Domain.Entities.Terminal;

namespace POS.Application.Commands.Terminal.Create;

public class CreateTerminalCommandHandler : IRequestHandler<CreateTerminalCommand, TerminalDto>
{
    private readonly ITerminalRepository _repository;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public CreateTerminalCommandHandler(ITerminalRepository repository, IUnitOfWork uow, IMapper mapper)
    {
        _repository = repository;
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<TerminalDto> Handle(CreateTerminalCommand request, CancellationToken cancellationToken)
    {
        var entity = _mapper.Map<Entity>(request.Dto);
        entity.StoreId = request.StoreId;
        await _repository.AddAsync(entity);
        await _uow.SaveChangesAsync(cancellationToken);
        return _mapper.Map<TerminalDto>(entity);
    }
}
