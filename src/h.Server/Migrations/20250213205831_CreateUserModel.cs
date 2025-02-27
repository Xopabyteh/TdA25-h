﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace h.Server.Migrations
{
    /// <inheritdoc />
    public partial class CreateUserModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UsersDbSet",
                columns: table => new
                {
                    Uuid = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "current_timestamp"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "current_timestamp"),
                    Username = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 320, nullable: false),
                    Roles = table.Column<string>(type: "TEXT", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
                    WinAmount = table.Column<ulong>(type: "INTEGER", nullable: false, defaultValue: 0ul),
                    LossAmount = table.Column<ulong>(type: "INTEGER", nullable: false, defaultValue: 0ul),
                    DrawAmount = table.Column<ulong>(type: "INTEGER", nullable: false, defaultValue: 0ul),
                    Elo_Rating = table.Column<ulong>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsersDbSet", x => x.Uuid);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UsersDbSet_Email",
                table: "UsersDbSet",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UsersDbSet_Username",
                table: "UsersDbSet",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UsersDbSet");
        }
    }
}
