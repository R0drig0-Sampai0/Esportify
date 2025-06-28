using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Esportify.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserProfileModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserProfiles",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    Bio = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    AvatarUrl = table.Column<string>(type: "TEXT", nullable: false),
                    Country = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    TwitchUrl = table.Column<string>(type: "TEXT", nullable: true),
                    YouTubeUrl = table.Column<string>(type: "TEXT", nullable: true),
                    TwitterUrl = table.Column<string>(type: "TEXT", nullable: true),
                    DiscordUrl = table.Column<string>(type: "TEXT", nullable: true),
                    TotalMatchesPlayed = table.Column<int>(type: "INTEGER", nullable: false),
                    TournamentsWon = table.Column<int>(type: "INTEGER", nullable: false),
                    TournamentsJoined = table.Column<int>(type: "INTEGER", nullable: false),
                    FavoriteGame = table.Column<string>(type: "TEXT", nullable: true),
                    FavoriteTeam = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProfiles", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_UserProfiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserProfiles");
        }
    }
}
