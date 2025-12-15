using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineGameStoreSystem.Migrations
{
    /// <inheritdoc />
    public partial class UpdateGame_Description : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsLike",
                table: "GameLikes");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Games",
                newName: "ShortDescription");

            migrationBuilder.AddColumn<string>(
                name: "DetailDescription",
                table: "Games",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DetailDescription",
                table: "Games");

            migrationBuilder.RenameColumn(
                name: "ShortDescription",
                table: "Games",
                newName: "Description");

            migrationBuilder.AddColumn<bool>(
                name: "IsLike",
                table: "GameLikes",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
