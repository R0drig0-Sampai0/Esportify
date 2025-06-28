using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Esportify.Migrations
{
    /// <inheritdoc />
    public partial class Added_UserProfile_Model : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Country",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Users");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "Users",
                type: "TEXT",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Users",
                type: "TEXT",
                maxLength: 256,
                nullable: false,
                defaultValue: "");
        }
    }
}
