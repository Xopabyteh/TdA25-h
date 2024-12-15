using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace h.Server.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GamesDbSet",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "date('now')"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "date('now')"),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Difficulty = table.Column<int>(type: "INTEGER", nullable: false),
                    GameState = table.Column<int>(type: "INTEGER", nullable: false),
                    Board_BoardMatrix = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GamesDbSet", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GamesDbSet");
        }
    }
}
