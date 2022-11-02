using EF.Support.Entities.Implements;

namespace Test.Entity;

public class NguyenEntity:Entity<Guid>
{
    public string Name { get; set; }
}

public class Testtiep : FullAuditedEntity<Guid>
{
    public string Name { get; set; }
}

