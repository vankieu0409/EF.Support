using EF.Support.Entities.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using EF.Support.Entities.Interfaces.Audited;
using System.Collections.ObjectModel;

namespace EF.Support.Repository;

public class Repository<TDbContext, TEntity>:IRepository<TDbContext, TEntity>,IDisposable,IEntity
    where TEntity : class, new()
    where TDbContext : DbContext, new()
{
    private readonly TDbContext _dbContext;

    public DbSet<TEntity> Entities { get; set; }

    public Repository()
    {
        this._dbContext = new TDbContext();
        this.Entities = this._dbContext.Set<TEntity>();
    }

    public TEntity Add(TEntity entity)
    {
        this.PreSaveChange(entity);
        return this.Entities.Add(entity).Entity;
    }

    public IEnumerable<TEntity> AddRange(IEnumerable<TEntity> entities)
    {
        Collection<TEntity> resullt = new Collection<TEntity>();
        foreach (var entity in entities)
        {
            Collection<TEntity> collection = resullt;
            collection.Add( this.Add(entity));
            collection = (Collection<TEntity>)null;
        }

        IEnumerable<TEntity> entityColletion = (IEnumerable<TEntity>)resullt;
        return entityColletion;
    }

    public TEntity Update(TEntity entity)
    {
        this.PreSaveChange(entity);
        return this.Entities.Update(entity).Entity;
    }

    public IEnumerable<TEntity> UpdateRange(IEnumerable<TEntity> entities)
    {
        Collection<TEntity> result = new Collection<TEntity>();
        foreach (TEntity entity in entities)
        {
            Collection<TEntity> collection = result;
            collection.Add(this.Update(entity));
            collection = (Collection<TEntity>)null;
        }
        IEnumerable<TEntity> entityCollection = (IEnumerable<TEntity>)result;
        result = (Collection<TEntity>)null;
        return entityCollection;
    }

    public TEntity Remove(TEntity entity)
    {
        if (!this.IsInheritsFrom(typeof(TEntity), typeof(IDeletionAuditedEntity)))
            return this.Entities.Remove(entity).Entity;
        PropertyInfo property = entity.GetType().GetProperty("IsDeleted");
        property.SetValue((object)entity, Convert.ChangeType((object)true, property.PropertyType), (object[])null);
        return  this.Update(entity);
    }

    public IEnumerable<TEntity> RemoveRange(IEnumerable<TEntity> entities)
    {
        if (this.IsInheritsFrom(typeof(TEntity), typeof(IDeletionAuditedEntity)))
        {
            IEnumerable<TEntity> removedEntities = entities.Select<TEntity, TEntity>((Func<TEntity, TEntity>)(entity =>
            {
                PropertyInfo property = entity.GetType().GetProperty("IsDeleted");
                property.SetValue((object)entity, Convert.ChangeType((object)true, property.PropertyType), (object[])null);
                return entity;
            }));
            IEnumerable<TEntity> entities1 = this.UpdateRange(removedEntities);
            return removedEntities;
        }
        this.Entities.RemoveRange(entities);
        return entities;
    }

    public virtual IQueryable<TEntity> AsQueryable() => (IQueryable<TEntity>)this._dbContext.Set<TEntity>();

    public int SaveChanges() => this._dbContext.SaveChanges();

    public void Dispose()
    {
        this._dbContext?.Dispose();
        GC.SuppressFinalize((object)this);
    }

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
