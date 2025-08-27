using System;

namespace PTM.Infrastructure.Repository;

public interface IBaseRepository<T> where T : class
{
    Task<T> AddAsync(T entity);
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task UpdateAsync(T entity);
    Task<T?> DeleteAsync(Guid id);
}
