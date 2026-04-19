using AutoMapper;
using MediatR;
using POS.Domain.Enums;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;

namespace POS.Application.Commands.Transaction.Update;

public class UpdateTransactionCommandHandler : IRequestHandler<UpdateTransactionCommand>
{
    private readonly ITransactionRepository _repository;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public UpdateTransactionCommandHandler(ITransactionRepository repository, IUnitOfWork uow, IMapper mapper)
    {
        _repository = repository;
        _uow = uow;
        _mapper = mapper;
    }

    public async Task Handle(UpdateTransactionCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id)
            ?? throw new KeyNotFoundException($"Transaction {request.Id} not found.");

        entity.Status = request.Dto.Status;
        entity.Notes = request.Dto.Notes;

        if (entity.Status == TransactionStatus.Completed && entity.CompletedAt is null)
            entity.CompletedAt = DateTimeOffset.UtcNow;

        _repository.Update(entity);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
