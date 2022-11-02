using EF.Support.Entities.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using EF.Support.RepositoryAsync;

namespace EF.Support.RepositoryAsync;

public class RepositoryAsync<TEntity> : RepositoryBaseAsync<TEntity> where TEntity : class, IEntity
{
    private readonly DbContext _primaryDbContext;
    private readonly DbContext _readOnlyDbContext;

    public RepositoryAsync(DbContext primaryDbContext, DbContext readOnlyDbContext) : base(primaryDbContext)
    {
        this._primaryDbContext = primaryDbContext ?? throw new ArgumentNullException(nameof(primaryDbContext));
        this._readOnlyDbContext = readOnlyDbContext ?? throw new ArgumentNullException(nameof(readOnlyDbContext));
    }

    public override IQueryable<TEntity> AsQueryable() => (IQueryable<TEntity>)this._readOnlyDbContext.Set<TEntity>();

    private void PreSaveChange(TEntity entity)
    {
        foreach (PropertyInfo property in entity.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            switch (property.GetValue((object)entity, (object[])null))
            {
                case DateTime dateTime2:
                    DateTime dateTime1 = this.Truncate(dateTime2, TimeSpan.FromSeconds(1.0));
                    property.SetValue((object)entity, Convert.ChangeType((object)dateTime1, property.PropertyType), (object[])null);
                    break;
                case DateTimeOffset dateTime3:
                    DateTimeOffset dateTimeOffset = this.Truncate(dateTime3, TimeSpan.FromSeconds(1.0));
                    property.SetValue((object)entity, Convert.ChangeType((object)dateTimeOffset, property.PropertyType), (object[])null);
                    break;
            }
        }
    }

    private DateTime Truncate(DateTime dateTime, TimeSpan timeSpan) => timeSpan == TimeSpan.Zero || dateTime == DateTime.MinValue || dateTime == DateTime.MaxValue ? dateTime : dateTime.AddTicks(-(dateTime.Ticks % timeSpan.Ticks));

    private DateTimeOffset Truncate(DateTimeOffset dateTime, TimeSpan timeSpan) => timeSpan == TimeSpan.Zero || dateTime == DateTimeOffset.MinValue || dateTime == DateTimeOffset.MaxValue ? dateTime : dateTime.AddTicks(-(dateTime.Ticks % timeSpan.Ticks));

    private bool IsInheritsFrom(object source, Type baseType)
    {
        Type type1 = source.GetType();
        if (type1 == (Type)null)
            return false;
        if (baseType == (Type)null)
            return type1.IsInterface || type1 == typeof(object);
        if (baseType.IsInterface)
            return ((IEnumerable<Type>)type1.GetInterfaces()).Contains<Type>(baseType);
        Type type2 = type1;
        if (!(type2 != (Type)null))
            return false;
        if (type2.BaseType == baseType)
            return true;
        Type baseType1 = type2.BaseType;
        return this.IsInheritsFrom(source, baseType1);
    }

    private bool IsInheritsFrom(Type sourceType, Type baseType)
    {
        if (sourceType == (Type)null)
            return false;
        if (baseType == (Type)null)
            return sourceType.IsInterface || sourceType == typeof(object);
        if (baseType.IsInterface)
            return ((IEnumerable<Type>)sourceType.GetInterfaces()).Contains<Type>(baseType);
        Type type = sourceType;
        if (!(type != (Type)null))
            return false;
        if (type.BaseType == baseType)
            return true;
        Type baseType1 = type.BaseType;
        return this.IsInheritsFrom(sourceType, baseType1);
    }
}
