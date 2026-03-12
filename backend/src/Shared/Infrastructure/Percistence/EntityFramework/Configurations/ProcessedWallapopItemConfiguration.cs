using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wallanoti.Src.Shared.Infrastructure.Percistence.EntityFramework.EntityModels;

namespace Wallanoti.Src.Shared.Infrastructure.Percistence.EntityFramework.Configurations;

public sealed class ProcessedWallapopItemConfiguration : IEntityTypeConfiguration<ProcessedWallapopItemEntity>
{
    public void Configure(EntityTypeBuilder<ProcessedWallapopItemEntity> builder)
    {
        builder.ToTable("ProcessedWallapopItems");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.HasIndex(x => new { x.AlertId, x.WallapopItemId }).IsUnique();
        builder.HasIndex(x => new { x.AlertId, x.ProcessedAtUtc }).IsDescending();

        builder.Property(x => x.ProcessedAtUtc)
            .HasConversion(dateTime => dateTime, dateTime => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc));

        builder.Property(x => x.StoredPrice)
            .HasColumnType("numeric(10,2)");
    }
}