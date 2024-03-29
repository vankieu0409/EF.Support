﻿namespace EF.Support.Entities.Interfaces.Audited;

public interface IModificationAuditedEntity
{

    DateTimeOffset ModifiedTime { get; }

    Guid? ModifiedBy { get; }
}