using POS.Domain.Entities;

namespace POS.Domain.Repositories;

public interface ICustomerRepository : IGenericRepository<Customer>
{
    Task<Customer?> GetByEmailOrPhoneAsync(string emailOrPhone);
}
