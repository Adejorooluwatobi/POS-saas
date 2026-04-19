using AutoMapper;
using MediatR;
using POS.Domain.Enums;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;

namespace POS.Application.Commands.TillSession.Update;

public class UpdateTillSessionCommandHandler : IRequestHandler<UpdateTillSessionCommand>
{
    private readonly ITillSessionRepository _repository;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public UpdateTillSessionCommandHandler(ITillSessionRepository repository, IUnitOfWork uow, IMapper mapper)
    {
        _repository = repository;
        _uow = uow;
        _mapper = mapper;
    }

    public async Task Handle(UpdateTillSessionCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id)
            ?? throw new KeyNotFoundException($"TillSession {request.Id} not found.");

        _mapper.Map(request.Dto, entity);

        if (entity.Status == SessionStatus.Closed && entity.ClosedAt is null)
        {
            entity.ClosedAt = DateTimeOffset.UtcNow;
            if (entity.ClosingCash.HasValue)
                entity.Variance = entity.ClosingCash - entity.ExpectedCash;
        }

        _repository.Update(entity);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
