using AutoMapper;
using MediatR;
using POS.Domain.Entities;
using POS.Domain.Enums;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;

namespace POS.Application.Commands.TillSession.Update;

public class UpdateTillSessionCommandHandler : IRequestHandler<UpdateTillSessionCommand>
{
    private readonly ITillSessionRepository _repository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public UpdateTillSessionCommandHandler(ITillSessionRepository repository, ITransactionRepository transactionRepository, IUnitOfWork uow, IMapper mapper)
    {
        _repository = repository;
        _transactionRepository = transactionRepository;
        _uow = uow;
        _mapper = mapper;
    }

    public async Task Handle(UpdateTillSessionCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id)
            ?? throw new KeyNotFoundException($"TillSession {request.Id} not found.");

        _mapper.Map(request.Dto, entity);

        if (entity.Status == SessionStatus.Closed)
        {
            if (entity.ClosedAt is null)
                entity.ClosedAt = DateTimeOffset.UtcNow;

            // Dynamically calculate expected cash at time of closing
            var transactions = await _transactionRepository.GetBySessionIdAsync(entity.Id);
            var cashPayments = transactions
                .SelectMany(t => t.Payments)
                .Where(p => p.Method == PaymentMethod.Cash)
                .Sum(p => p.Amount);

            entity.ExpectedCash = entity.OpeningFloat + cashPayments;

            if (entity.ClosingCash.HasValue)
                entity.Variance = entity.ClosingCash - entity.ExpectedCash;
        }

        _repository.Update(entity);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
