using Microsoft.EntityFrameworkCore;
using PTM.Infrastructure.Database;

namespace PTM.Infrastructure.Repository;

public class BaseRepository<T> : IBaseRepository<T> where T : class
{
    private readonly AppDbContext context;
    private readonly DbSet<T> dbSet;

    public BaseRepository(AppDbContext context)
    {
        this.context = context;
        dbSet = context.Set<T>();
    }
    public async Task AddAsync(T entity)
    {
        await dbSet.AddAsync(entity);
        await context.SaveChangesAsync();
    }
    
    public async Task<T?> GetByIdAsync(Guid id)
    {
        return await dbSet.FindAsync(id);
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await dbSet.ToListAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            dbSet.Remove(entity);
            await context.SaveChangesAsync();
        }
    }

    public async Task UpdateAsync(T entity)
    {
        dbSet.Update(entity);
        await context.SaveChangesAsync();
    }
}
