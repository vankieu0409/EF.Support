using EF.Support.Entities.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EF.Support.RepositoryAsync;

public class SingleDbRepositoryAsync<TEntity> : IRepositoryAsync<TEntity>, IDisposable where TEntity : class, IEntity
{
    private readonly DbContext _dbContext;

    public DbSet<TEntity> Entities { get; }

    public SingleDbRepositoryAsync(DbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        Entities = _dbContext.Set<TEntity>();
    }

    public async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        => (await Entities.AddAsync(entity, cancellationToken)).Entity;

    public Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        => Entities.AddRangeAsync(entities, cancellationToken);

    public Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        => Task.FromResult(Entities.Update(entity).Entity);

    public Task UpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        Entities.UpdateRange(entities);
        return Task.CompletedTask;
    }

    public Task<TEntity> RemoveAsync(TEntity entity, CancellationToken cancellationToken = default)
        => Task.FromResult(Entities.Remove(entity).Entity);

    public Task RemoveRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        Entities.RemoveRange(entities);
        return Task.CompletedTask;
    }

    public IQueryable<TEntity> AsQueryable() => _dbContext.Set<TEntity>();

    public IQueryable<TEntity> AsNoTrackingQueryable() => _dbContext.Set<TEntity>().AsNoTracking();

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _dbContext.SaveChangesAsync(cancellationToken);

    public void Dispose()
    {
        _dbContext?.Dispose();
        GC.SuppressFinalize(this);
    }
}
