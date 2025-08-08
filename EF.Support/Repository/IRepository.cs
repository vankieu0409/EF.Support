using EF.Support.Entities.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EF.Support.Repository;

public interface IRepository<TDbContext, TEntity> : IDisposable
    where TEntity : class, IEntity
    where TDbContext : DbContext
{
    DbSet<TEntity> Entities { get; }

    TEntity Add(TEntity entity);
    void AddRange(IEnumerable<TEntity> entities);

    TEntity Update(TEntity entity);
    void UpdateRange(IEnumerable<TEntity> entities);

    TEntity Remove(TEntity entity);
    void RemoveRange(IEnumerable<TEntity> entities);

    IQueryable<TEntity> AsQueryable();
    IQueryable<TEntity> AsNoTrackingQueryable();

    int SaveChanges();
}