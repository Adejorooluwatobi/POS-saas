using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using POS.Domain.Common;

namespace POS.Domain.Repositories;

public interface IGenericRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<PagedResult<T>> GetPagedAsync(int pageNumber, int pageSize);
    Task AddAsync(T entity);
    void Update(T entity);
    void Delete(T entity);
}
