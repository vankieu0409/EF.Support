using EF.Support.Entities.Interfaces.Audited;
using EF.Support.Entities.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace EF.Support.Entities.Implements;

public class FullAuditedEntity<TKey> :
    FullAuditedEntity,
    IAuditedEntity<TKey>,
    ICreationAuditedEntity,
    IModificationAuditedEntity,
    IDeletionAuditedEntity,
    IEntity<TKey>,
    IEntity
{
    [Key]
    public required TKey Id { get; set; }
}