using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace h.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddFinishedRankedGameModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FinishedRankedGames",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LastBoardState_BoardMatrix = table.Column<string>(type: "TEXT", nullable: false),
                    PlayedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Player1Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Player2Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Player1Symbol = table.Column<int>(type: "INTEGER", nullable: false),
                    Player2Symbol = table.Column<int>(type: "INTEGER", nullable: false),
                    Player1RemainingTimer = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    Player2RemainingTimer = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    WinnerId = table.Column<Guid>(type: "TEXT", nullable: true),
                    IsDraw = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinishedRankedGames", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserToFinishedRankedGames",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    FinishedRankedGameId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserToFinishedRankedGames", x => new { x.UserId, x.FinishedRankedGameId });
                    table.ForeignKey(
                        name: "FK_UserToFinishedRankedGames_FinishedRankedGames_FinishedRankedGameId",
                        column: x => x.FinishedRankedGameId,
                        principalTable: "FinishedRankedGames",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserToFinishedRankedGames_UsersDbSet_UserId",
                        column: x => x.UserId,
                        principalTable: "UsersDbSet",
                        principalColumn: "Uuid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserToFinishedRankedGames_FinishedRankedGameId",
                table: "UserToFinishedRankedGames",
                column: "FinishedRankedGameId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserToFinishedRankedGames");

            migrationBuilder.DropTable(
                name: "FinishedRankedGames");
        }
    }
}
