namespace EF.Support.Entities.Interfaces.Audited;

public interface IAuditedEntity<TKey> :
    ICreationAuditedEntity,
    IModificationAuditedEntity,
    IDeletionAuditedEntity,
    IEntity<TKey>,
    IEntity
{
    
}