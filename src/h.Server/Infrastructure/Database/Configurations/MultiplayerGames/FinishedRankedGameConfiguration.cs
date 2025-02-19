using h.Server.Entities.MultiplayerGames;
using h.Server.Infrastructure.Database.Configurations.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace h.Server.Infrastructure.Database.Configurations.MultiplayerGames;

public class FinishedRankedGameConfiguration : IEntityTypeConfiguration<FinishedRankedGame>
{
    public void Configure(EntityTypeBuilder<FinishedRankedGame> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();
    
        // Board class has a 2D array property, which is not supported by EF Core
        //, serialize to json
        builder.OwnsOne(x => x.LastBoardState, b =>
        {
            b.Property(bx => bx.BoardMatrix)
                .HasConversion(new BoardMatrixConverter());

            b.WithOwner();
        });

        builder.Property(x => x.PlayedAt)
            .IsRequired();

        builder.Property(x => x.Player1Id)
            .IsRequired();

        builder.Property(x => x.Player2Id)
            .IsRequired();

        builder.Property(x => x.Player1Symbol)
            .IsRequired();

        builder.Property(x => x.Player2Symbol)
            .IsRequired();

        builder.Property(x => x.Player1RemainingTimer)
            .IsRequired();

        builder.Property(x => x.Player2RemainingTimer)
            .IsRequired();

        builder.Property(x => x.WinnerId)
            .IsRequired(false);

        builder.Property(x => x.IsDraw)
            .IsRequired();
    }
}
