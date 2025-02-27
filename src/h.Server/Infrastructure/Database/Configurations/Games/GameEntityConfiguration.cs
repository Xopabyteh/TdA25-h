﻿using h.Server.Entities.Games;
using h.Server.Infrastructure.Database.Configurations.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace h.Server.Infrastructure.Database.Configurations.Games;

public class GameEntityConfiguration : IEntityTypeConfiguration<Game>
{
    public void Configure(EntityTypeBuilder<Game> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(x => x.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("current_timestamp");
        builder.Property(x => x.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("current_timestamp");

        builder.Property(x => x.Name).IsRequired();

        builder.Property(x => x.Difficulty).IsRequired();
        builder.Property(x => x.GameState).IsRequired();

        // Board class has a 2D array property, which is not supported by EF Core
        //, serialize to json
        builder.OwnsOne(x => x.Board, b =>
        {
            b.Property(bx => bx.BoardMatrix)
                .HasConversion(new BoardMatrixConverter());

            b.WithOwner();
        });
    }
}
