using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace h.Server.Migrations
{
    /// <inheritdoc />
    public partial class RetypeUlongEloToIn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "WinAmount",
                table: "UsersDbSet",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(ulong),
                oldType: "INTEGER",
                oldDefaultValue: 0ul);

            migrationBuilder.AlterColumn<int>(
                name: "LossAmount",
                table: "UsersDbSet",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(ulong),
                oldType: "INTEGER",
                oldDefaultValue: 0ul);

            migrationBuilder.AlterColumn<int>(
                name: "DrawAmount",
                table: "UsersDbSet",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(ulong),
                oldType: "INTEGER",
                oldDefaultValue: 0ul);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<ulong>(
                name: "WinAmount",
                table: "UsersDbSet",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0ul,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<ulong>(
                name: "LossAmount",
                table: "UsersDbSet",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0ul,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<ulong>(
                name: "DrawAmount",
                table: "UsersDbSet",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0ul,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldDefaultValue: 0);
        }
    }
}
