using EF.Support.Entities.Interfaces.Audited;
using EF.Support.Entities.Interfaces;

namespace EF.Support.Entities.Implements;

public class FullAuditedEntity :
    Entity,
    IAuditedEntity,
    ICreationAuditedEntity,
    IModificationAuditedEntity,
    IDeletionAuditedEntity,
    IEntity
{
    public DateTimeOffset CreatedTime { get; }
    public Guid? CreatedBy { get; }
    public DateTimeOffset ModifiedTime { get; }
    public Guid? ModifiedBy { get; }
    public bool IsDeleted { get; }
    public Guid? DeletedBy { get; }
    public DateTimeOffset DeletedTime { get; }
}