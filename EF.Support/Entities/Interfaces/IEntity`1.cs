namespace EF.Support.Entities.Interfaces;

public interface IEntity<TKey> : IEntity
{
    TKey Id { get; }
}