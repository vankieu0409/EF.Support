using EF.Support.Entities.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EF.Support.RepositoryAsync;

public interface IRepositoryAsync<TEntity> : IDisposable where TEntity : class, IEntity
{
    DbSet<TEntity> Entities { get; }

    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task UpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    Task<TEntity> RemoveAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task RemoveRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    IQueryable<TEntity> AsQueryable();
    IQueryable<TEntity> AsNoTrackingQueryable();

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}