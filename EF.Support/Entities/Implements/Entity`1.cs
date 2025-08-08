using System.ComponentModel.DataAnnotations;
using EF.Support.Entities.Interfaces;

namespace EF.Support.Entities.Implements;

public abstract class Entity<TKey> : Entity, IEntity<TKey>, IEntity
{
    [Key]
    public required TKey Id { get; set; }
}