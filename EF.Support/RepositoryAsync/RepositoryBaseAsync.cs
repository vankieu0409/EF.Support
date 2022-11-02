using EF.Support.Entities.Interfaces.Audited;
using EF.Support.Entities.Interfaces;
using EF.Support.RepositoryAsync;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace EF.Support.RepositoryAsync;

public class RepositoryBaseAsync<TEntity> : IRepositoryAsync<TEntity>, IDisposable where TEntity : class, IEntity
{
    private readonly DbContext _dbContext;

    public DbSet<TEntity> Entities { get; set; }

    public RepositoryBaseAsync(DbContext dbContext)
    {
        //if (dbContext == null)
        //{
        //    DbContext local1 = dbContext; throw new ArgumentNullException("dbContext");
        //}
        //this._dbContext = dbContext;
        //this.Entities = this._dbContext.Set<TEntity>();
        this._dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        this.Entities = this._dbContext.Set<TEntity>();
    }

    //[AsyncStateMachine((Type)typeof(<AddAsync >d__6))]
    //public async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = new CancellationToken())
    //{
    //    < AddAsync > d__6<TEntity> d__;
    //    d__.<> t__builder = AsyncTaskMethodBuilder<TEntity>.Create();
    //    d__.<> 4__this = (RepositoryBase<TEntity>)this;
    //    d__.entity = entity;
    //    d__.cancellationToken = cancellationToken;
    //    d__.<> 1__state = -1;
    //    d__.<> t__builder.Start << AddAsync > d__6 < TEntity >> (ref d__);
    //    return d__.<> t__builder.get_Task();
    //}

    public async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default(CancellationToken))
    {
        this.PreSaveChange(entity);
        return (await this.Entities.AddAsync(entity, cancellationToken)).Entity;
    }

    public async Task<IEnumerable<TEntity>> AddRangeAsync(
      IEnumerable<TEntity> entities,
      CancellationToken cancellationToken = default(CancellationToken))
    {
        Collection<TEntity> resullt = new Collection<TEntity>();
        foreach (var entity in entities)
        {
            Collection<TEntity> collection = resullt;
            collection.Add(await this.AddAsync(entity, cancellationToken));
            collection = (Collection<TEntity>)null;
        }

        IEnumerable<TEntity> entityColletion = (IEnumerable<TEntity>)resullt;
        return entityColletion;
    }

    public async Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default(CancellationToken))
    {
        this.PreSaveChange(entity);
        return await Task.FromResult<TEntity>(this.Entities.Update(entity).Entity);
    }

    public async Task<IEnumerable<TEntity>> UpdateRangeAsync(
      IEnumerable<TEntity> entities,
      CancellationToken cancellationToken = default(CancellationToken))
    {

        Collection<TEntity> result = new Collection<TEntity>();
        foreach (TEntity entity in entities)
        {
            Collection<TEntity> collection = result;
            collection.Add(await this.UpdateAsync(entity, cancellationToken));
            collection = (Collection<TEntity>)null;
        }
        IEnumerable<TEntity> entityCollection = (IEnumerable<TEntity>)result;
        result = (Collection<TEntity>)null;
        return entityCollection;
    }

    public async Task<TEntity> RemoveAsync(TEntity entity, CancellationToken cancellationToken = default(CancellationToken))
    {
        if (!this.IsInheritsFrom(typeof(TEntity), typeof(IDeletionAuditedEntity)))
            return await Task.FromResult<TEntity>(this.Entities.Remove(entity).Entity);
        PropertyInfo property = entity.GetType().GetProperty("IsDeleted");
        property.SetValue((object)entity, Convert.ChangeType((object)true, property.PropertyType), (object[])null);
        return await this.UpdateAsync(entity, cancellationToken);
    }

    public async Task<IEnumerable<TEntity>> RemoveRangeAsync(
      IEnumerable<TEntity> entities,
      CancellationToken cancellationToken = default(CancellationToken))
    {
        if (this.IsInheritsFrom(typeof(TEntity), typeof(IDeletionAuditedEntity)))
        {
            IEnumerable<TEntity> removedEntities = entities.Select<TEntity, TEntity>((Func<TEntity, TEntity>)(entity =>
            {
                PropertyInfo property = entity.GetType().GetProperty("IsDeleted");
                property.SetValue((object)entity, Convert.ChangeType((object)true, property.PropertyType), (object[])null);
                return entity;
            }));
            IEnumerable<TEntity> entities1 = await this.UpdateRangeAsync(removedEntities);
            return await Task.FromResult<IEnumerable<TEntity>>(removedEntities);
        }
        this.Entities.RemoveRange(entities);
        return await Task.FromResult<IEnumerable<TEntity>>(entities);
    }

    public virtual IQueryable<TEntity> AsQueryable() => (IQueryable<TEntity>)this._dbContext.Set<TEntity>();

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken)) => await this._dbContext.SaveChangesAsync(cancellationToken);

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
