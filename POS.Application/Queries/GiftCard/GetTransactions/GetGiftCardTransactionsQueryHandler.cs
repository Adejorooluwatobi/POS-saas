using AutoMapper;
using MediatR;
using POS.Application.DTOs;
using POS.Domain.Repositories;

namespace POS.Application.Queries.GiftCard.GetTransactions;

public class GetGiftCardTransactionsQueryHandler : IRequestHandler<GetGiftCardTransactionsQuery, IEnumerable<GiftCardTransactionDto>>
{
    private readonly IGiftCardTransactionRepository _repository;
    private readonly IMapper _mapper;

    public GetGiftCardTransactionsQueryHandler(IGiftCardTransactionRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<GiftCardTransactionDto>> Handle(GetGiftCardTransactionsQuery request, CancellationToken cancellationToken)
    {
        var transactions = await _repository.GetByCardIdAsync(request.GiftCardId);
        return _mapper.Map<IEnumerable<GiftCardTransactionDto>>(transactions);
    }
}
