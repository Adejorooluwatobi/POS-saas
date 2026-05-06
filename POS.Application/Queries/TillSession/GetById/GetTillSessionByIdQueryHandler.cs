using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using POS.Application.DTOs;
using POS.Domain.Entities;
using POS.Domain.Repositories;

namespace POS.Application.Queries.TillSession.GetById;

public class GetTillSessionByIdQueryHandler : IRequestHandler<GetTillSessionByIdQuery, TillSessionDto?>
{
    private readonly ITillSessionRepository _repository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IMapper _mapper;

    public GetTillSessionByIdQueryHandler(ITillSessionRepository repository, ITransactionRepository transactionRepository, IMapper mapper)
    {
        _repository = repository;
        _transactionRepository = transactionRepository;
        _mapper = mapper;
    }

    public async Task<TillSessionDto?> Handle(GetTillSessionByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id);
        if (entity == null) return null;

        var dto = _mapper.Map<TillSessionDto>(entity);

        // Dynamically calculate expected cash if session is still open
        if (entity.Status == POS.Domain.Enums.SessionStatus.Open)
        {
            var transactions = await _transactionRepository.GetBySessionIdAsync(entity.Id);
            var cashPayments = transactions
                .SelectMany(t => t.Payments)
                .Where(p => p.Method == POS.Domain.Enums.PaymentMethod.Cash)
                .Sum(p => p.Amount);

            dto.ExpectedCash = entity.OpeningFloat + cashPayments;
        }

        return dto;
    }
}
