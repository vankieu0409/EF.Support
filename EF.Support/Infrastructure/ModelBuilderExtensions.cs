using EF.Support.Entities.Interfaces.Audited;
using Microsoft.EntityFrameworkCore;

namespace EF.Support.Infrastructure;

public static class ModelBuilderExtensions
{
    public static void ApplySoftDeleteGlobalFilter(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;
            if (typeof(IDeletionAuditedEntity).IsAssignableFrom(clrType))
            {
                var parameter = System.Linq.Expressions.Expression.Parameter(clrType, "e");
                var prop = System.Linq.Expressions.Expression.Property(parameter, nameof(IDeletionAuditedEntity.IsDeleted));
                var notDeleted = System.Linq.Expressions.Expression.Equal(prop, System.Linq.Expressions.Expression.Constant(false));
                var filter = System.Linq.Expressions.Expression.Lambda(notDeleted, parameter);
                entityType.SetQueryFilter(filter);
            }
        }
    }

    public static void ApplyAuditPrecision(this ModelBuilder modelBuilder)
    {
        // Set precision for audit timestamps to seconds to avoid reflection-based truncation
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;
            if (typeof(ICreationAuditedEntity).IsAssignableFrom(clrType))
            {
                modelBuilder.Entity(clrType).Property(nameof(ICreationAuditedEntity.CreatedTime)).HasPrecision(0);
            }
            if (typeof(IModificationAuditedEntity).IsAssignableFrom(clrType))
            {
                modelBuilder.Entity(clrType).Property(nameof(IModificationAuditedEntity.ModifiedTime)).HasPrecision(0);
            }
            if (typeof(IDeletionAuditedEntity).IsAssignableFrom(clrType))
            {
                modelBuilder.Entity(clrType).Property(nameof(IDeletionAuditedEntity.DeletedTime)).HasPrecision(0);
            }
        }
    }
}
