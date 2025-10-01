using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WallapopNotification.Shared._1_Domain.ValueObject;
using WallapopNotification.Users._1_Domain.Models;

namespace WallapopNotification.Shared._3_Infrastructure.Percistence.EntityFramework.Configurations;

public sealed class AlertsConfiguration : IEntityTypeConfiguration<Alerts._1_Domain.Models.Alert>
{
    public void Configure(EntityTypeBuilder<Alerts._1_Domain.Models.Alert> builder)
    {
        var urlConverter = new ValueConverter<Url, string>(
            v => v.ToString(), // Convierte Url a string
            v => Url.Create(v) // Convierte string a Url
        );

        builder.ToTable("alerts");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.UserId).HasColumnName("userid");
        builder.Property(x => x.Name).HasColumnName("name");
        builder.Property(x => x.Url)
            .HasColumnName("url")
            .HasConversion(urlConverter);

        builder.Property(x => x.CreatedAt)
            .HasConversion(dateTime => dateTime, dateTime => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc))
            .HasColumnName("createdat");

        builder.Property(x => x.LastSearch)
            .HasConversion(
                dateTime => dateTime,
                dateTime => dateTime.HasValue ? DateTime.SpecifyKind(dateTime.Value, DateTimeKind.Utc) : null
            )
            .HasColumnName("lastsearch");
    }
}