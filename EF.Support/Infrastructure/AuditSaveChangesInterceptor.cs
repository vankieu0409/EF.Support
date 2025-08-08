using EF.Support.Entities.Interfaces.Audited;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EF.Support.Infrastructure;

public class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly Func<Guid?> _getUserId;

    public AuditSaveChangesInterceptor(Func<Guid?> getUserId)
    {
        _getUserId = getUserId;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (eventData.Context is null) return base.SavingChanges(eventData, result);
        ApplyAudit(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            ApplyAudit(eventData.Context);
        }
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void ApplyAudit(DbContext context)
    {
        var userId = _getUserId();
        var now = DateTimeOffset.UtcNow;

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.Entity is ICreationAuditedEntity created && entry.State == EntityState.Added)
            {
                created.CreatedTime = now;
                created.CreatedBy = userId;
            }

            if (entry.Entity is IModificationAuditedEntity modified && (entry.State == EntityState.Modified))
            {
                modified.ModifiedTime = now;
                modified.ModifiedBy = userId;
            }

            if (entry.Entity is IDeletionAuditedEntity deleted && entry.State == EntityState.Deleted)
            {
                // Convert hard delete to soft delete
                entry.State = EntityState.Modified;
                deleted.IsDeleted = true;
                deleted.DeletedTime = now;
                deleted.DeletedBy = userId;
            }
        }
    }
}
