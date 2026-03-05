using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wallanoti.Src.Users.Domain.Models;

namespace Wallanoti.Src.Shared.Infrastructure.Percistence.EntityFramework.Configurations;

public sealed class UsersConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.HasKey(x => x.Id);

        builder.OwnsOne(x => x.VerificationCode)
            .Property(x => x.Value)
            .HasColumnName("VerificationCode");
    }
}