using System.Threading;
using System.Threading.Tasks;

namespace POS.Domain.Interfaces;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
