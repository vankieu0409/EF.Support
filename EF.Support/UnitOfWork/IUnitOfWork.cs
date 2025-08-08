using Microsoft.EntityFrameworkCore;

namespace EF.Support.UnitOfWork;

public interface IUnitOfWork<TDbContext> : IDisposable where TDbContext : DbContext
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    int SaveChanges();
    Task ExecuteInTransactionAsync(Func<Task> action, CancellationToken cancellationToken = default);
}
