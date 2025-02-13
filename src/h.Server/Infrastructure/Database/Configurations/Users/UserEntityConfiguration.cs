using h.Server.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace h.Server.Infrastructure.Database.Configurations.Users;

public class UserEntityConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(x => x.Uuid);
        builder.Property(x => x.Uuid).ValueGeneratedOnAdd();

        builder.Property(x => x.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("current_timestamp");
        builder.Property(x => x.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("current_timestamp");
    
        builder.Property(x => x.Username)
            .HasMaxLength(256)
            .IsRequired();
        builder.HasIndex(x => x.Username)
            .IsUnique();

        builder.Property(x => x.Email)
            .HasMaxLength(320)
            .IsRequired();
        builder.HasIndex(x => x.Email)
            .IsUnique();

        builder.Property(x => x.PasswordHash)
            .IsRequired();

        builder.ComplexProperty(x => x.Elo, b=>
        {
            b.IsRequired();
            b.Property(bx => bx.Rating)
                .IsRequired();
        });

        builder.Property(x => x.WinAmount)
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(x => x.LossAmount)
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(x => x.DrawAmount)
            .HasDefaultValue(0)
            .IsRequired();

        // Not supported by sqlite
        builder.PrimitiveCollection(x => x.Roles);
    }
}
