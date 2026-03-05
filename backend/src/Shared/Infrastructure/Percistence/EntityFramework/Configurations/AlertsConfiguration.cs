using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Wallanoti.Src.Alerts.Domain.Models;
using Wallanoti.Src.Shared.Domain.ValueObjects;

namespace Wallanoti.Src.Shared.Infrastructure.Percistence.EntityFramework.Configurations;

public sealed class AlertsConfiguration : IEntityTypeConfiguration<Alert>
{
    public void Configure(EntityTypeBuilder<Alert> builder)
    {
        var urlConverter = new ValueConverter<Url, string>(
            v => v.ToString(), // Convierte Url a string
            v => Url.Create(v) // Convierte string a Url
        );

        builder.ToTable("alerts");
        builder.Property(x => x.UserId)
            .IsRequired();
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Url)
            .HasConversion(urlConverter);
        
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.CreatedAt)
            .HasConversion(dateTime => dateTime, dateTime => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc));

        builder.Property(x => x.UpdatedAt)
            .HasConversion(
                dateTime => dateTime,
                dateTime => dateTime.HasValue ? DateTime.SpecifyKind(dateTime.Value, DateTimeKind.Utc) : null
            );

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);
    }
}