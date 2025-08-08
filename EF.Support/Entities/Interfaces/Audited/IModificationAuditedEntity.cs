namespace EF.Support.Entities.Interfaces.Audited;

public interface IModificationAuditedEntity
{
    DateTimeOffset ModifiedTime { get; set; }
    Guid? ModifiedBy { get; set; }
}