using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Customer?> GetByEmailOrPhoneAsync(string emailOrPhone)
    {
        return await _dbSet
            .Include(c => c.GiftCards)
            .FirstOrDefaultAsync(c => 
                (string.Equals(c.Email, emailOrPhone) || string.Equals(c.Phone, emailOrPhone))
                && c.IsActive);
    }
}
