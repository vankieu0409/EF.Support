using System.Diagnostics.CodeAnalysis;
using EF.Support.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Test.Entity;

namespace Test;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext([NotNullAttribute] DbContextOptions<ApplicationDbContext> options) : base(options)
    {
        this.ChangeTracker.LazyLoadingEnabled = false;
    }

    public ApplicationDbContext()
    {
       
    }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.LogTo(Console.WriteLine);
       // optionsBuilder.UseSqlServer("Data Source=DESKTOP-2FLV48L; Initial Catalog=sdfghjkdfghjdfgh;Persist Security Info=True;User ID=kieu96;Password=123");
    }

    public DbSet<NguyenEntity> NguyenEntities { get; set; }
    public DbSet<Testtiep> Testtieps { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplySoftDeleteGlobalFilter();
        modelBuilder.ApplyAuditPrecision();

        modelBuilder.Entity<NguyenEntity>().ToTable("NGUYEN");
        modelBuilder.Entity<NguyenEntity>(entity => { entity.HasKey(c => c.Id); });

        modelBuilder.Entity<Testtiep>().ToTable("EntityTest");
        modelBuilder.Entity<Testtiep>(entity => { entity.HasKey(c => c.Id); });
    }
}