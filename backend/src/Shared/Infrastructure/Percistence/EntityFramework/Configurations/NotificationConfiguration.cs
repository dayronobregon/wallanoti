using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wallanoti.Src.Shared.Infrastructure.Percistence.EntityFramework.EntityModels;
using Wallanoti.Src.Users.Domain.Models;

namespace Wallanoti.Src.Shared.Infrastructure.Percistence.EntityFramework.Configurations;

public sealed class NotificationConfiguration : IEntityTypeConfiguration<NotificationEntity>
{
    public void Configure(EntityTypeBuilder<NotificationEntity> builder)
    {
        builder.ToTable("notifications");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.HasOne<User>()
            .WithOne()
            .HasForeignKey<NotificationEntity>(x => x.UserId);

        builder.HasIndex(x => new { x.UserId, x.Url, x.CreatedAt });
        
        builder.Property(x => x.CreatedAt)
            .HasConversion(dateTime => dateTime, dateTime => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc));
    }
}
