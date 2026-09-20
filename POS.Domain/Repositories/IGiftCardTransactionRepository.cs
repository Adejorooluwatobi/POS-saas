using POS.Domain.Entities;

namespace POS.Domain.Repositories;

public interface IGiftCardTransactionRepository : IGenericRepository<GiftCardTransaction>
{
    Task<List<GiftCardTransaction>> GetByCardIdAsync(Guid cardId);
}
