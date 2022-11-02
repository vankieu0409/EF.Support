using EF.Support.Repository;
using Test.Entity;

namespace Test.Repository;

public interface IFullAuditedEntityRepository : IRepository<ApplicationDbContext, Testtiep>
{
    
}