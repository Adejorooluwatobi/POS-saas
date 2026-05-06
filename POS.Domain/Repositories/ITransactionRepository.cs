using POS.Domain.Entities;

namespace POS.Domain.Repositories;

public interface ITransactionRepository : IGenericRepository<Transaction>
{
    Task<IEnumerable<Transaction>> GetBySessionIdAsync(Guid sessionId);
}
