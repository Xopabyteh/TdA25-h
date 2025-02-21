﻿// <auto-generated />
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using h.Server.Infrastructure.Database;

#nullable disable

namespace h.Server.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20250221102357_AddAuditLog")]
    partial class AddAuditLog
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "9.0.0");

            modelBuilder.Entity("h.Server.Entities.AuditLog.AuditLogEntry", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Arguments")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .HasDefaultValueSql("current_timestamp");

                    b.Property<string>("Format")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("IPAdressV4")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("AuditLogEntries");
                });

            modelBuilder.Entity("h.Server.Entities.Games.Game", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .HasDefaultValueSql("current_timestamp");

                    b.Property<int>("Difficulty")
                        .HasColumnType("INTEGER");

                    b.Property<int>("GameState")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("UpdatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .HasDefaultValueSql("current_timestamp");

                    b.HasKey("Id");

                    b.ToTable("GamesDbSet");
                });

            modelBuilder.Entity("h.Server.Entities.MultiplayerGames.FinishedRankedGame", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsDraw")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("PlayedAt")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("Player1Id")
                        .HasColumnType("TEXT");

                    b.Property<TimeSpan>("Player1RemainingTimer")
                        .HasColumnType("TEXT");

                    b.Property<int>("Player1Symbol")
                        .HasColumnType("INTEGER");

                    b.Property<Guid>("Player2Id")
                        .HasColumnType("TEXT");

                    b.Property<TimeSpan>("Player2RemainingTimer")
                        .HasColumnType("TEXT");

                    b.Property<int>("Player2Symbol")
                        .HasColumnType("INTEGER");

                    b.Property<Guid?>("WinnerId")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("FinishedRankedGames");
                });

            modelBuilder.Entity("h.Server.Entities.MultiplayerGames.UserToFinishedRankedGame", b =>
                {
                    b.Property<Guid>("UserId")
                        .HasColumnType("TEXT");

                    b.Property<int>("FinishedRankedGameId")
                        .HasColumnType("INTEGER");

                    b.HasKey("UserId", "FinishedRankedGameId");

                    b.HasIndex("FinishedRankedGameId");

                    b.ToTable("UserToFinishedRankedGames");
                });

            modelBuilder.Entity("h.Server.Entities.Users.User", b =>
                {
                    b.Property<Guid>("Uuid")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("BannedFromRankedMatchmakingAt")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .HasDefaultValueSql("current_timestamp");

                    b.Property<int>("DrawAmount")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasDefaultValue(0);

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(320)
                        .HasColumnType("TEXT");

                    b.Property<int>("LossAmount")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasDefaultValue(0);

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.PrimitiveCollection<string>("Roles")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("UpdatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .HasDefaultValueSql("current_timestamp");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.Property<int>("WinAmount")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasDefaultValue(0);

                    b.ComplexProperty<Dictionary<string, object>>("Elo", "h.Server.Entities.Users.User.Elo#ThinkDifferentElo", b1 =>
                        {
                            b1.Property<int>("Rating")
                                .HasColumnType("INTEGER");
                        });

                    b.HasKey("Uuid");

                    b.HasIndex("Email")
                        .IsUnique();

                    b.HasIndex("Username")
                        .IsUnique();

                    b.ToTable("UsersDbSet");
                });

            modelBuilder.Entity("h.Server.Entities.Games.Game", b =>
                {
                    b.OwnsOne("h.Server.Entities.Games.GameBoard", "Board", b1 =>
                        {
                            b1.Property<Guid>("GameId")
                                .HasColumnType("TEXT");

                            b1.Property<string>("BoardMatrix")
                                .IsRequired()
                                .HasColumnType("TEXT");

                            b1.HasKey("GameId");

                            b1.ToTable("GamesDbSet");

                            b1.WithOwner()
                                .HasForeignKey("GameId");
                        });

                    b.Navigation("Board")
                        .IsRequired();
                });

            modelBuilder.Entity("h.Server.Entities.MultiplayerGames.FinishedRankedGame", b =>
                {
                    b.OwnsOne("h.Server.Entities.Games.GameBoard", "LastBoardState", b1 =>
                        {
                            b1.Property<int>("FinishedRankedGameId")
                                .HasColumnType("INTEGER");

                            b1.Property<string>("BoardMatrix")
                                .IsRequired()
                                .HasColumnType("TEXT");

                            b1.HasKey("FinishedRankedGameId");

                            b1.ToTable("FinishedRankedGames");

                            b1.WithOwner()
                                .HasForeignKey("FinishedRankedGameId");
                        });

                    b.Navigation("LastBoardState")
                        .IsRequired();
                });

            modelBuilder.Entity("h.Server.Entities.MultiplayerGames.UserToFinishedRankedGame", b =>
                {
                    b.HasOne("h.Server.Entities.MultiplayerGames.FinishedRankedGame", "FinishedRankedGame")
                        .WithMany("UserToFinishedRankedGames")
                        .HasForeignKey("FinishedRankedGameId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("h.Server.Entities.Users.User", "User")
                        .WithMany("UserToFinishedRankedGames")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("FinishedRankedGame");

                    b.Navigation("User");
                });

            modelBuilder.Entity("h.Server.Entities.MultiplayerGames.FinishedRankedGame", b =>
                {
                    b.Navigation("UserToFinishedRankedGames");
                });

            modelBuilder.Entity("h.Server.Entities.Users.User", b =>
                {
                    b.Navigation("UserToFinishedRankedGames");
                });
#pragma warning restore 612, 618
        }
    }
}
