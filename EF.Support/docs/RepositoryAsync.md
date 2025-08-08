## Hướng dẫn RepositoryAsync và SingleDbRepositoryAsync (EF Core 8, .NET 8)

Tài liệu này mô tả chi tiết 2 lớp repository bất đồng bộ trong gói EF.Support, cách đăng ký DI, cách dùng, và các lưu ý quan trọng.

### Chọn lớp nào?

- RepositoryAsync<TEntity>
  - Dùng được với 1 hoặc 2 DbContext.
  - Khi truyền 2 DbContext (primaryDbContext, readOnlyDbContext) sẽ hỗ trợ read/write split: ghi vào primary, đọc từ read-only.
  - Khi truyền 1 DbContext, cả đọc/ghi đều dùng cùng context.

- SingleDbRepositoryAsync<TEntity>
  - Đơn giản hơn, luôn dùng 1 DbContext cho cả đọc và ghi.
  - Thích hợp khi không cần read/write split.

### Đăng ký DI (ServiceCollection)

```csharp
// Single DB (đọc/ghi cùng DB)
services.AddDbContext<AppDbContext>(opt =>
{
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Default"));
    opt.AddInterceptors(new AuditSaveChangesInterceptor(() => currentUserId));
});

// RepositoryAsync dùng 1 DbContext
services.AddScoped(typeof(EF.Support.RepositoryAsync.RepositoryAsync<>));
services.AddScoped(typeof(EF.Support.RepositoryAsync.SingleDbRepositoryAsync<>));

// Read/Write split (tuỳ chọn)
services.AddDbContext<AppReadOnlyDbContext>(opt =>
{
    opt.UseSqlServer(builder.Configuration.GetConnectionString("ReadOnly"));
});

// Nếu muốn RepositoryAsync dùng 2 DbContext, đăng ký factory/service riêng:
services.AddScoped(provider =>
{
    var primary = provider.GetRequiredService<AppDbContext>();
    var readOnly = provider.GetRequiredService<AppReadOnlyDbContext>();
    // Ví dụ tiêm sẵn RepositoryAsync<User>
    return new EF.Support.RepositoryAsync.RepositoryAsync<User>(primary, readOnly);
});
```

Gợi ý: trong thực tế, bạn có thể bọc đăng ký trên bằng lớp factory hoặc extension để tái sử dụng và tránh lặp.

#### Đăng ký qua Interface (khuyên dùng)

- Mặc định (Single DB): ánh xạ `IRepositoryAsync<TEntity>` -> `SingleDbRepositoryAsync<TEntity>`

```csharp
// Dùng interface ở mọi nơi trong ứng dụng
services.AddScoped(typeof(EF.Support.RepositoryAsync.IRepositoryAsync<>),
                  typeof(EF.Support.RepositoryAsync.SingleDbRepositoryAsync<>));

// Inject:
public class UserService
{
    private readonly EF.Support.RepositoryAsync.IRepositoryAsync<User> _repo;
    public UserService(EF.Support.RepositoryAsync.IRepositoryAsync<User> repo) => _repo = repo;
}
```

- Read/Write split theo từng entity: đăng ký cụ thể cho mỗi TEntity cần split

```csharp
// Ví dụ chỉ split cho User, các entity khác vẫn dùng SingleDb
services.AddScoped<EF.Support.RepositoryAsync.IRepositoryAsync<User>>(sp =>
    new EF.Support.RepositoryAsync.RepositoryAsync<User>(
        sp.GetRequiredService<AppDbContext>(),
        sp.GetRequiredService<AppReadOnlyDbContext>()));

// Những entity còn lại vẫn qua open-generic mặc định SingleDb
services.AddScoped(typeof(EF.Support.RepositoryAsync.IRepositoryAsync<>),
                  typeof(EF.Support.RepositoryAsync.SingleDbRepositoryAsync<>));
```

Lưu ý: Do constructor của `RepositoryAsync<TEntity>` nhận tham số kiểu `DbContext`, DI không thể tự nội suy từ `AppDbContext`/`AppReadOnlyDbContext`. Vì vậy với chế độ read/write split, hãy đăng ký theo từng entity như trên hoặc tạo extension method để bọc lại việc đăng ký.

### API bề mặt (cả 2 lớp)

- AddAsync(entity, CancellationToken)
- AddRangeAsync(entities, CancellationToken)
- UpdateAsync(entity, CancellationToken)
- UpdateRangeAsync(entities, CancellationToken)
- RemoveAsync(entity, CancellationToken)
- RemoveRangeAsync(entities, CancellationToken)
- AsQueryable()           // truy vấn có tracking
- AsNoTrackingQueryable() // truy vấn không tracking (đọc nhanh, không lưu trạng thái)
- SaveChangesAsync(CancellationToken)

### Ví dụ: dùng SingleDbRepositoryAsync

```csharp
public class UserService
{
    private readonly SingleDbRepositoryAsync<User> _repo;

    public UserService(SingleDbRepositoryAsync<User> repo)
    {
        _repo = repo;
    }

    public async Task<Guid> CreateAsync(User user, CancellationToken ct)
    {
        await _repo.AddRangeAsync(new[] { user }, ct); // hoặc AddAsync(user, ct)
        await _repo.SaveChangesAsync(ct);
        return user.Id;
    }

    public Task<List<User>> GetActiveAsync(CancellationToken ct)
        => _repo.AsNoTrackingQueryable().Where(u => !u.IsDeleted).ToListAsync(ct);
}
```

### Ví dụ: dùng RepositoryAsync với 2 DbContext (read/write split)

```csharp
public class ReportQuery
{
    private readonly RepositoryAsync<Order> _repo; // đã được tiêm với (primary, readOnly)

    public ReportQuery(RepositoryAsync<Order> repo)
    {
        _repo = repo;
    }

    public Task<List<Order>> GetLatestAsync(CancellationToken ct)
        => _repo.AsNoTrackingQueryable()
                .OrderByDescending(o => o.CreatedTime)
                .Take(50)
                .ToListAsync(ct);
}

public class OrderCommand
{
    private readonly RepositoryAsync<Order> _repo; // cùng instance như trên (ghi vào primary)

    public OrderCommand(RepositoryAsync<Order> repo)
    {
        _repo = repo;
    }

    public async Task MarkPaidAsync(Guid id, CancellationToken ct)
    {
        var order = await _repo.AsQueryable().FirstOrDefaultAsync(o => o.Id == id, ct)
                    ?? throw new KeyNotFoundException();
        order.IsPaid = true;
        await _repo.SaveChangesAsync(ct);
    }
}
```

Lưu ý: khi dùng read/write split có thể gặp độ trễ sao chép dữ liệu (replication lag). Nếu cần tính nhất quán mạnh (đọc sau ghi phải thấy), hãy đọc từ primary hoặc dùng giao dịch/buộc đọc nhất quán.

### Best practices

- Ưu tiên AsNoTrackingQueryable() cho luồng chỉ-đọc để giảm chi phí tracking.
- Truyền CancellationToken vào các API async để hỗ trợ huỷ bỏ.
- Gọi SaveChangesAsync sau khi Add/Update/Remove; với nhiều thao tác, gộp lại để giảm round-trip.
- Kết hợp AuditSaveChangesInterceptor và global filter soft-delete để tự động hoá audit + ẩn dữ liệu bị xoá mềm.
- Đặt lifetime DbContext/Repository là Scoped (mặc định trong ASP.NET Core) cho mỗi request.

### Lỗi thường gặp

- Không tiêm đúng DbContext vào constructor dẫn đến lỗi thiếu tham số.
- Quên gọi SaveChangesAsync nên không có thay đổi trên DB.
- Dùng AsQueryable() trong luồng đọc số lượng lớn gây chi phí tracking không cần thiết.

### Di chuyển từ Repository đồng bộ

- Đổi các phương thức Add/Update/Remove sang AddAsync/UpdateAsync/RemoveAsync.
- Đổi SaveChanges() sang SaveChangesAsync().
- Thêm await và CancellationToken ở luồng bất đồng bộ.

### Kiểm thử (Unit Test)

- Dùng InMemoryDatabase hoặc Sqlite in-memory với EF Core để test nhanh.
- Tiêm DbContext test vào RepositoryAsync/SingleDbRepositoryAsync qua DI hoặc tạo trực tiếp trong test.

---

Tham khảo thêm: README chính trong thư mục EF.Support để xem cách bật interceptor audit và global filter soft-delete.
