using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wallanoti.Src.Alerts.Domain.Entities;
using Wallanoti.Src.Shared.Domain.Aggregates;

namespace Wallanoti.Src.Shared.Infrastructure.Percistence.EntityFramework.Configurations;

public class ProcessedItemConfiguration : IEntityTypeConfiguration&lt;ProcessedItem&gt;
{
    public void Configure(EntityTypeBuilder&lt;ProcessedItem&gt; builder)
    {
        builder.ToTable("ProcessedItems");

        builder.HasKey(p =&gt; new { p.AlertId, p.ItemId });

        builder.Property(p =&gt; p.LastModifiedAtMs);

        builder.Property(p =&gt; p.RowVersion)
            .IsRowVersion();

        builder.HasIndex(p =&gt; p.AlertId);

        builder.HasIndex(p =&gt; p.LastModifiedAtMs);
    }
}