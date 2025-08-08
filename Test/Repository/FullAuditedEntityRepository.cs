using Test.Entity;
using Test;
using EF.Support.Repository;
using Microsoft.EntityFrameworkCore;

namespace Test.Repository;

public class FullAuditedEntityRepository:Repository<ApplicationDbContext,Testtiep>,IFullAuditedEntityRepository
{
	public FullAuditedEntityRepository(ApplicationDbContext dbContext) : base(dbContext)
	{
	}
}