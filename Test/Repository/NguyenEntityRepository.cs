using EF.Support.Repository;
using EF.Support.RepositoryAsync;
using Microsoft.EntityFrameworkCore;
using Test.Entity;

namespace Test.Repository;

public class NguyenEntityRepository:RepositoryAsync<NguyenEntity>,INguyenEntityRepository
{
    private readonly ApplicationDbContext _context;

    public NguyenEntityRepository( ApplicationDbContext context) : base(context, context)
    {
        _context = context?? throw new ArgumentNullException(nameof(context));
    }
}