namespace EF.Support.Entities.Interfaces.Audited;

public interface IDeletionAuditedEntity
{
    bool IsDeleted { get; }

    Guid? DeletedBy { get; }

    DateTimeOffset DeletedTime { get; }
}