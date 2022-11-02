using EF.Support.Entities.Interfaces.Audited;
using EF.Support.Entities.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;
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
    [Key, Column(Order = 0), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public TKey Id { get; set; }
}