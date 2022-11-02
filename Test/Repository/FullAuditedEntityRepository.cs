using Test.Entity;
using EF.Support.Repository;
using Microsoft.EntityFrameworkCore;

namespace Test.Repository;

public class FullAuditedEntityRepository:Repository<ApplicationDbContext,Testtiep>,IFullAuditedEntityRepository
{
}