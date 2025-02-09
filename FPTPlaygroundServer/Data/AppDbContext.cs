using FPTPlaygroundServer.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace FPTPlaygroundServer.Data;

public class AppDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Account> Accounts { get; set; } = default!;
    public DbSet<AccountVerify> AccountVerifies { get; set; } = default!;
    public DbSet<Device> Devices { get; set; } = default!;
    public DbSet<Server> Servers { get; set; } = default!;
    public DbSet<Specialize> Specializes { get; set; } = default!;
    public DbSet<User> Users { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<Enum>().HaveConversion<string>();
    }
}
