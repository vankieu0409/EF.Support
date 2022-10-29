using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using EF.Support.Entities.Interfaces;

namespace EF.Support.Entities.Implements;

public abstract class Entity<TKey> : Entity, IEntity<TKey>, IEntity
{
    [Key]
    [Column(Order = 0)]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public virtual TKey Id { get; protected set; }
}