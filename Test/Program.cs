using EF.Support.Infrastructure;
using EF.Support.RepositoryAsync;

using Microsoft.EntityFrameworkCore;

using System.Security.Claims;

using Test;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();
//builder.Services.AddScoped<AuditSaveChangesInterceptor>(sp =>
//{
//    var accessor = sp.GetRequiredService<IHttpContextAccessor>();

//    return new AuditSaveChangesInterceptor(() =>
//    {
//        var user = accessor.HttpContext?.User;
//        if (user?.Identity?.IsAuthenticated != true) return (Guid?)null;

//        // Ưu tiên các claim thường gặp
//        var idStr = user.FindFirstValue(ClaimTypes.NameIdentifier)
//                    ?? user.FindFirstValue("sub")
//                    ?? user.FindFirstValue("uid");

//        return Guid.TryParse(idStr, out var id) ? id : (Guid?)null;
//    });
//});
builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default"));
   // options.AddInterceptors(sp.GetRequiredService<AuditSaveChangesInterceptor>());
});
builder.Services.AddScoped<DbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
builder.Services.AddScoped(typeof(EF.Support.Repository.IRepository<,>), typeof(EF.Support.Repository.Repository<,>));
builder.Services.AddScoped(typeof(EF.Support.UnitOfWork.IUnitOfWork<>), typeof(EF.Support.UnitOfWork.UnitOfWork<>));
builder.Services.AddScoped(typeof(IRepositoryAsync<>), typeof(SingleDbRepositoryAsync<>));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();
app.MapControllers();

app.Run();
