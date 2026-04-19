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

    public async Task<Customer?> GetByEmailOrPhoneAsync(string emailOrPhone)
    {
        return await _dbSet.FirstOrDefaultAsync(c => 
            (string.Equals(c.Email, emailOrPhone) || string.Equals(c.Phone, emailOrPhone))
            && c.IsActive);
    }
}
