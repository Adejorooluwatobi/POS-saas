using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using POS.Domain.Common;
using POS.Domain.Entities;
using POS.Domain.Repositories;
using POS.Infrastructure.Data;

namespace POS.Infrastructure.Repositories;

public class CustomerRepository : GenericRepository<Customer>, ICustomerRepository
{
    public CustomerRepository(RetailOsDbContext context) : base(context) { }

    public override async Task<Customer?> GetByIdAsync(Guid id)
    {
        return await _dbSet
            .Include(c => c.GiftCards)
                .ThenInclude(g => g.IssuingStore)
            .Include(c => c.RegisteredStore)
            .Include(c => c.RegisteredByStaff)
            .Include(c => c.Transactions)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public override async Task<PagedResult<Customer>> GetPagedAsync(int pageNumber, int pageSize)
    {
        var count = await _dbSet.CountAsync();
        var items = await _dbSet
            .Include(c => c.RegisteredStore)
            .Include(c => c.RegisteredByStaff)
            .Include(c => c.Transactions)
            .OrderByDescending(c => c.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<Customer>
        {
            Items = items,
            TotalCount = count,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<Customer?> GetByEmailOrPhoneAsync(string emailOrPhone)
    {
        return await _dbSet
            .Include(c => c.GiftCards)
                .ThenInclude(g => g.IssuingStore)
            .Include(c => c.RegisteredStore)
            .Include(c => c.RegisteredByStaff)
            .FirstOrDefaultAsync(c => 
                (string.Equals(c.Email, emailOrPhone) || string.Equals(c.Phone, emailOrPhone))
                && c.IsActive);
    }
}
