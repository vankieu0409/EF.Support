namespace EF.Support.Entities.Interfaces.Audited;

public interface ICreationAuditedEntity
{
    DateTimeOffset CreatedTime { get; }
    Guid? CreatedBy { get; }
}