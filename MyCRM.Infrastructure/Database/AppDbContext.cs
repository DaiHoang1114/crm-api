using Microsoft.EntityFrameworkCore;
using MyCRM.Domain.Entities;

namespace MyCRM.Infrastructure.Database;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<Todo> Todos => Set<Todo>();
}