using EF.Support.Entities.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EF.Support.Repository;

public interface IRepository<TDbContext,TEntity>:IDisposable
where TEntity : class, new ()
where TDbContext : DbContext, new ()
{

    DbSet<TEntity> Entities { get; set; }

    TEntity Add(TEntity entity);

    IEnumerable<TEntity> AddRange(IEnumerable<TEntity> entities);

    TEntity Update(TEntity entity);

    IEnumerable<TEntity> UpdateRange(IEnumerable<TEntity> entities);

    TEntity Remove(TEntity entity);

    IEnumerable<TEntity> RemoveRange(IEnumerable<TEntity> entities);

    IQueryable<TEntity> AsQueryable();

    int SaveChanges();
}