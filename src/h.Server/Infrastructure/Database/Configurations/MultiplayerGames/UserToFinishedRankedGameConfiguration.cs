using h.Server.Entities.MultiplayerGames;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace h.Server.Infrastructure.Database.Configurations.MultiplayerGames;

public class UserToFinishedRankedGameConfiguration : IEntityTypeConfiguration<UserToFinishedRankedGame>
{
    public void Configure(EntityTypeBuilder<UserToFinishedRankedGame> builder)
    {
        builder.HasKey(x => new { x.UserId, x.FinishedRankedGameId });

        builder.HasOne(ufg => ufg.User)
            .WithMany(u => u.UserToFinishedRankedGames)
            .HasForeignKey(ufg => ufg.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ufg => ufg.FinishedRankedGame)
            .WithMany(frg => frg.UserToFinishedRankedGames)
            .HasForeignKey(ufg => ufg.FinishedRankedGameId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
