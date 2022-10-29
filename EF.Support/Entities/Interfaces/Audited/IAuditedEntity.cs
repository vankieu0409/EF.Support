namespace EF.Support.Entities.Interfaces.Audited;

public interface IAuditedEntity :
ICreationAuditedEntity,
IModificationAuditedEntity,
IDeletionAuditedEntity,
IEntity
{

}