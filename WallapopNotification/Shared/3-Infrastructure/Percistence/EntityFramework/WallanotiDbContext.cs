using Microsoft.EntityFrameworkCore;
using WallapopNotification.Alerts._1_Domain.Models;
using WallapopNotification.Shared._3_Infrastructure.Percistence.EntityFramework.Configurations;
using WallapopNotification.Users._1_Domain.Models;

namespace WallapopNotification.Shared._3_Infrastructure.Percistence.EntityFramework;

public sealed class WallanotiDbContext:DbContext
{
    
    public WallanotiDbContext(DbContextOptions<WallanotiDbContext> options) : base(options)
    {
    }
    
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Alert> Alerts { get; set; } = null!;
    
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UsersConfiguration());
        modelBuilder.ApplyConfiguration(new AlertsConfiguration());
        
        base.OnModelCreating(modelBuilder);
    }
}