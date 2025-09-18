using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Esportify.Migrations
{
    /// <inheritdoc />
    public partial class TeamTableChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TeamName",
                table: "Teams",
                newName: "Name");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Teams",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "TeamMembers",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "TeamMembers");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Teams",
                newName: "TeamName");
        }
    }
}
