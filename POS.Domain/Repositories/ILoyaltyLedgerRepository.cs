using POS.Domain.Entities;

namespace POS.Domain.Repositories;

public interface ILoyaltyLedgerRepository : IGenericRepository<LoyaltyLedgerEntry>
{
    Task<IEnumerable<LoyaltyLedgerEntry>> GetByCustomerIdAsync(Guid customerId);
}
