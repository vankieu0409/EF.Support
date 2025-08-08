using EF.Support.Entities.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EF.Support.Repository;

public class Repository<TDbContext, TEntity> : IRepository<TDbContext, TEntity>, IDisposable
    where TEntity : class, IEntity
    where TDbContext : DbContext
{
    private readonly TDbContext _dbContext;

    public DbSet<TEntity> Entities { get; }

    public Repository(TDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        Entities = _dbContext.Set<TEntity>();
    }

    public TEntity Add(TEntity entity) => Entities.Add(entity).Entity;

    public void AddRange(IEnumerable<TEntity> entities) => Entities.AddRange(entities);

    public TEntity Update(TEntity entity) => Entities.Update(entity).Entity;

    public void UpdateRange(IEnumerable<TEntity> entities) => Entities.UpdateRange(entities);

    public TEntity Remove(TEntity entity) => Entities.Remove(entity).Entity;

    public void RemoveRange(IEnumerable<TEntity> entities) => Entities.RemoveRange(entities);

    public virtual IQueryable<TEntity> AsQueryable() => _dbContext.Set<TEntity>();

    public virtual IQueryable<TEntity> AsNoTrackingQueryable() => _dbContext.Set<TEntity>().AsNoTracking();

    public int SaveChanges() => _dbContext.SaveChanges();

    public void Dispose()
    {
        _dbContext?.Dispose();
        GC.SuppressFinalize(this);
    }
}
