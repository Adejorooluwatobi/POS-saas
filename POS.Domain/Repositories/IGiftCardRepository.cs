using POS.Domain.Entities;

namespace POS.Domain.Repositories;

public interface IGiftCardRepository : IGenericRepository<GiftCard>
{
    Task<GiftCard?> GetByCardNumberAsync(Guid tenantId, string cardNumber);
    Task<GiftCard?> GetByIdWithDetailsAsync(Guid id);
}
