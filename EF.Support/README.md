# EF.Support (optimized for EF Core 8 + DI)

This refactor aligns with .NET 8 and EF Core 8 best practices, removes reflection-based mutations, and introduces DI-first repositories, global soft-delete, audit interceptor, and Unit of Work support.

## What changed
- EF Core 8 dependency.
- DI-based repositories (no `new TDbContext()` constraint).
- Global query filter for soft-delete (IsDeleted == false).
- Audit interceptor that sets Created/Modified/Deleted fields and converts hard delete to soft delete.
- `AsNoTrackingQueryable()` for read paths; EF native AddRange/UpdateRange/RemoveRange.
- Removed Identity attribute on generic `Id`; mark as `required`.
- Added Unit of Work abstraction.

## Integration guide

### 1) Register DbContexts and EF infrastructure
```csharp
services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(connStr);
    options.AddInterceptors(new AuditSaveChangesInterceptor(() => currentUserId));
});

// Optional read-only context (for read/write split)
services.AddDbContext<AppReadOnlyDbContext>(options =>
{
    options.UseSqlServer(readOnlyConnStr);
});
```

In your `DbContext`:
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);
    modelBuilder.ApplySoftDeleteGlobalFilter();
    modelBuilder.ApplyAuditPrecision();
}
```

### 2) Register repositories and UoW
```csharp
services.AddScoped(typeof(EF.Support.Repository.IRepository<,>), typeof(EF.Support.Repository.Repository<,>));
services.AddScoped(typeof(EF.Support.RepositoryAsync.RepositoryBaseAsync<>));
services.AddScoped(typeof(EF.Support.RepositoryAsync.RepositoryAsync<>));
services.AddScoped(typeof(EF.Support.UnitOfWork.IUnitOfWork<>), typeof(EF.Support.UnitOfWork.UnitOfWork<>));
```

### 3) Use repositories (sync)
```csharp
public class UserService
{
    private readonly IRepository<AppDbContext, User> _repo;

    public UserService(IRepository<AppDbContext, User> repo)
    {
        _repo = repo;
    }

    public void Create(User user)
    {
        _repo.Add(user);
        _repo.SaveChanges();
    }

    public IEnumerable<User> GetActive()
        => _repo.AsNoTrackingQueryable().ToList();
}
```

### 4) Use repositories (async + read/write split)
```csharp
public class UserQueryService
{
    private readonly RepositoryAsync<User> _repo;

    public UserQueryService(RepositoryAsync<User> repo)
    {
        _repo = repo;
    }

    public async Task<User?> GetByIdAsync(Guid id)
        => await _repo.AsNoTrackingQueryable().FirstOrDefaultAsync(x => x.Id == id);
}
```

### 5) Unit of Work
```csharp
public class OrderService
{
    private readonly IRepository<AppDbContext, Order> _orders;
    private readonly IUnitOfWork<AppDbContext> _uow;

    public OrderService(IRepository<AppDbContext, Order> orders, IUnitOfWork<AppDbContext> uow)
    {
        _orders = orders;
        _uow = uow;
    }

    public Task PlaceAsync(Order order)
        => _uow.ExecuteInTransactionAsync(async () =>
        {
            _orders.Add(order);
            // add more repo calls here
        });
}
```

## Notes
- Soft-delete filter hides deleted rows globally; use `IgnoreQueryFilters()` for administrative scenarios.
- Read/Write split can observe replication lag; use primary context for strict consistency or enforce read-after-write via transaction or retry.
- Precision is set to seconds for audit timestamps to match DB defaults and avoid spurious updates.

## Tài liệu chi tiết RepositoryAsync
- Xem hướng dẫn chi tiết về RepositoryAsync và SingleDbRepositoryAsync: `docs/RepositoryAsync.md`
