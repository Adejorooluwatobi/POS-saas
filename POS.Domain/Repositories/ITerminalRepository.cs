using POS.Domain.Entities;

namespace POS.Domain.Repositories;

public interface ITerminalRepository : IGenericRepository<Terminal>
{
    Task<IEnumerable<Terminal>> GetByStoreAsync(Guid storeId);
    Task<Terminal?> GetByPairingCodeAsync(string pairingCode);
}
