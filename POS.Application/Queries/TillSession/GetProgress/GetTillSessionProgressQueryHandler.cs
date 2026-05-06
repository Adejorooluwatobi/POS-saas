using MediatR;
using Microsoft.EntityFrameworkCore;
using POS.Application.DTOs;
using POS.Domain.Enums;
using POS.Domain.Repositories;

namespace POS.Application.Queries.TillSession.GetProgress;

public class GetTillSessionProgressQueryHandler : IRequestHandler<GetTillSessionProgressQuery, TillSessionProgressDto>
{
    private readonly ITillSessionRepository _repository;
    private readonly ITransactionRepository _transactionRepository;

    public GetTillSessionProgressQueryHandler(ITillSessionRepository repository, ITransactionRepository transactionRepository)
    {
        _repository = repository;
        _transactionRepository = transactionRepository;
    }

    public async Task<TillSessionProgressDto> Handle(GetTillSessionProgressQuery request, CancellationToken cancellationToken)
    {
        var session = await _repository.GetByIdAsync(request.SessionId)
            ?? throw new KeyNotFoundException($"Session {request.SessionId} not found.");

        var transactions = await _transactionRepository.GetBySessionIdAsync(request.SessionId);

        var dto = new TillSessionProgressDto
        {
            SessionId = session.Id,
            OpeningFloat = session.OpeningFloat,
            TotalSales = transactions.Sum(t => t.GrandTotal),
            TotalTax = transactions.Sum(t => t.TaxTotal),
            TotalDiscount = transactions.Sum(t => t.DiscountTotal),
            TransactionCount = transactions.Count(),
            ExpectedCash = session.OpeningFloat + transactions
                .Where(t => t.Payments.Any(p => p.Method == PaymentMethod.Cash))
                .Sum(t => t.GrandTotal)
        };

        // Note: This logic assumes a single payment method per transaction for simplicity in progress tracking
        // In a more complex system, we'd sum the individual payments.
        
        return dto;
    }
}
