using Microsoft.EntityFrameworkCore;
using Wallanoti.Src.Alerts.Domain.Models;
using Wallanoti.Src.Shared.Infrastructure.Percistence.EntityFramework.Configurations;
using Wallanoti.Src.Shared.Infrastructure.Percistence.EntityFramework.EntityModels;
using Wallanoti.Src.Users.Domain.Models;

namespace Wallanoti.Src.Shared.Infrastructure.Percistence.EntityFramework;

public sealed class WallanotiDbContext : DbContext
{
    public WallanotiDbContext(DbContextOptions<WallanotiDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Alert> Alerts { get; set; } = null!;
    public DbSet<AlertCounter.Domain.AlertCounter> AlertCounters { get; set; } = null!;
    public DbSet<NotificationEntity> Notifications { get; set; } = null!;
    public DbSet<ProcessedWallapopItemEntity> ProcessedWallapopItems { get; set; } = null!;
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UsersConfiguration());
        modelBuilder.ApplyConfiguration(new AlertsConfiguration());
        modelBuilder.ApplyConfiguration(new AlertCounterConfiguration());
        modelBuilder.ApplyConfiguration(new NotificationConfiguration());
        modelBuilder.ApplyConfiguration(new ProcessedWallapopItemConfiguration());

        base.OnModelCreating(modelBuilder);
    }
}