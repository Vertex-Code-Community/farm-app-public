using System.Linq.Expressions;
using FarmApp.DataAccessLayer.Repositories.Interfaces;
using FarmApp.Entities.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FarmApp.DataAccessLayer.Repositories;

public class GenericRepository<TEntity, TDb, TId> : IGenericRepository<TEntity, TId> 
    where TEntity : class, IBaseEntity<TId>
    where TDb : Microsoft.EntityFrameworkCore.DbContext
{
    private readonly TDb _context;
    public DbSet<TEntity> DbSet { get; }

    protected GenericRepository(TDb context)
    {
        _context = context;
        DbSet = context.Set<TEntity>();
    }

    public async Task CreateAsync(TEntity item)
    {
        await DbSet.AddAsync(item);
        await _context.SaveChangesAsync();
    }

    public async Task CreateRangeAsync(IEnumerable<TEntity> item)
    {
        await DbSet.AddRangeAsync(item);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(TEntity item)
    {
        DbSet.Remove(item);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteRangeAsync(IEnumerable<TEntity> items)
    {
        DbSet.RemoveRange(items);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<TEntity>> GetAllAsync()
    {
        return await DbSet.AsNoTracking().ToListAsync();
    }

    public async Task UpdateAsync(TEntity item)
    {
        _context.Entry(item).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public Task<TEntity?> GetByIdAsync(TId id)
    {
        return DbSet.AsNoTracking().FirstOrDefaultAsync(x => x.Id.Equals(id));
    }

    public Task<TEntity?> GetByIdForUpdateAsync(TId id)
    {
        return DbSet.FirstOrDefaultAsync(x => x.Id.Equals(id));
    }

    public Task<TEntity?> GetByIdAsync<TPrevProperty>(TId id, 
        Expression<Func<TEntity, IEnumerable<TPrevProperty>>> navigationPropertyPath)
    {
        return DbSet
            .AsNoTracking()
            .Include(navigationPropertyPath)
            .FirstOrDefaultAsync(x => x.Id.Equals(id));
    }
    
    public Task<TEntity?> GetByIdAsync(TId id, params Expression<Func<TEntity, object>>[] navigationPropertyPaths)
    {
        var query = DbSet.AsNoTracking();
        
        foreach (var navigationPropertyPath in navigationPropertyPaths)
            query = query.Include(navigationPropertyPath);
        
        return query.FirstOrDefaultAsync(x => x.Id.Equals(id));
    }

    public Task<TEntity?> GetByIdAsync<TPrevProperty, TProperty>(TId id, 
        Expression<Func<TEntity, TPrevProperty>> navigationPropertyPath, 
        Expression<Func<TPrevProperty, TProperty>> thenNavigationPropertyPath)
    {
        return DbSet
            .AsNoTracking()
            .Include(navigationPropertyPath)
            .ThenInclude(thenNavigationPropertyPath)
            .FirstOrDefaultAsync(x => x.Id.Equals(id));
    }
    
    public Task<TEntity?> GetByIdAsync<TPrevProperty, TProperty>(TId id, 
        Expression<Func<TEntity, IEnumerable<TPrevProperty>>> navigationPropertyPath, 
        Expression<Func<TPrevProperty, TProperty>> thenNavigationPropertyPath)
    {
        return DbSet
            .AsNoTracking()
            .Include(navigationPropertyPath)
            .ThenInclude(thenNavigationPropertyPath)
            .FirstOrDefaultAsync(x => x.Id.Equals(id));
    }

    public async Task AddRangeAsync(IEnumerable<TEntity> item)
    {
        await DbSet.AddRangeAsync(item);
        await _context.SaveChangesAsync();
    }

    public Task<int> GetCountAsync()
    {
        return DbSet.CountAsync();
    }
    public virtual async Task<int> GetCountAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await DbSet.AsNoTracking().Where(predicate).CountAsync();
    }
    
    public Task<List<TEntity>> GetAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return DbSet.Where(predicate).ToListAsync();
    }

    public Task<List<TEntity>> GetAsync<TPrevProperty>(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TPrevProperty>> navigationPropertyPath)
    {
        return DbSet.Where(predicate).Include(navigationPropertyPath).ToListAsync();
    }

    public async Task<List<TEntity>> GetAsync(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] navigationPropertyPaths)
    {
        var query = DbSet.AsNoTracking().Where(predicate);
        
        foreach (var navigationPropertyPath in navigationPropertyPaths)
            query = query.Include(navigationPropertyPath);
        
        return await query.ToListAsync();
    }

    public void Detach(TEntity item)
    {
        _context.Entry(item).State = EntityState.Detached;
    }
}

