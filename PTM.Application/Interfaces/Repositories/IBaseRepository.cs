using System;
using System.Linq.Expressions;

namespace PTM.Infrastructure.Repository;

public interface IBaseRepository<T> where T : class
{
    Task<T> AddAsync(T entity);
    Task<T?> GetByIdAsync(Guid id);
    Task<T?> GetByIdAsync(Guid id, params Expression<Func<T, object>>[] includes);
    Task<IEnumerable<T>> GetAllAsync();
    Task UpdateAsync(T entity);
    Task<T?> DeleteAsync(Guid id);
}
