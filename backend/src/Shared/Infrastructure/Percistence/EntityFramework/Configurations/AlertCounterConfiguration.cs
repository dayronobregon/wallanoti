using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wallanoti.Src.Alerts.Domain.Models;

namespace Wallanoti.Src.Shared.Infrastructure.Percistence.EntityFramework.Configurations;

public sealed class AlertCounterConfiguration : IEntityTypeConfiguration<AlertCounter.Domain.AlertCounter>
{
    public void Configure(EntityTypeBuilder<AlertCounter.Domain.AlertCounter> builder)
    {
        builder.ToTable("alert_counters");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.AlertId).IsRequired();

        builder.HasOne<Alert>()
            .WithOne()
            .HasForeignKey<AlertCounter.Domain.AlertCounter>(x => x.AlertId);
    }
}