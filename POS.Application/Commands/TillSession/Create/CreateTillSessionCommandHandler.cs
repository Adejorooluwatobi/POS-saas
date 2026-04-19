using AutoMapper;
using MediatR;
using POS.Application.DTOs;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;
using Entity = POS.Domain.Entities.TillSession;

namespace POS.Application.Commands.TillSession.Create;

public class CreateTillSessionCommandHandler : IRequestHandler<CreateTillSessionCommand, TillSessionDto>
{
    private readonly ITillSessionRepository _repository;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ITenantContext _tenantContext;

    public CreateTillSessionCommandHandler(ITillSessionRepository repository, IUnitOfWork uow, IMapper mapper, ITenantContext tenantContext)
    {
        _repository = repository;
        _uow = uow;
        _mapper = mapper;
        _tenantContext = tenantContext;
    }

    public async Task<TillSessionDto> Handle(CreateTillSessionCommand request, CancellationToken cancellationToken)
    {
        var entity = _mapper.Map<Entity>(request.Dto);
        entity.StaffId = _tenantContext.UserId!.Value;

        await _repository.AddAsync(entity);
        await _uow.SaveChangesAsync(cancellationToken);
        return _mapper.Map<TillSessionDto>(entity);
    }
}
