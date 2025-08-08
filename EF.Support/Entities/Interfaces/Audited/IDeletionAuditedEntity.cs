namespace EF.Support.Entities.Interfaces.Audited;

public interface IDeletionAuditedEntity
{
    bool IsDeleted { get; set; }
    Guid? DeletedBy { get; set; }
    DateTimeOffset DeletedTime { get; set; }
}