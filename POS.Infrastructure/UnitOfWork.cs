using System.Threading;
using System.Threading.Tasks;
using POS.Domain.Interfaces;
using POS.Infrastructure.Data;

namespace POS.Infrastructure;

public class UnitOfWork : IUnitOfWork
{
    private readonly RetailOsDbContext _context;

    public UnitOfWork(RetailOsDbContext context) => _context = context;

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}
