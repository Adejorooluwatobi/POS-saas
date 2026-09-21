using MediatR;
using POS.Application.DTOs;

namespace POS.Application.Queries.GiftCard.GetTransactions;

public record GetGiftCardTransactionsQuery(Guid GiftCardId) : IRequest<IEnumerable<GiftCardTransactionDto>>;
