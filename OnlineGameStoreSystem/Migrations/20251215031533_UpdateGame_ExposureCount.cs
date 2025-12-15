using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineGameStoreSystem.Migrations
{
    /// <inheritdoc />
    public partial class UpdateGame_ExposureCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ExposureCount",
                table: "Games",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExposureCount",
                table: "Games");
        }
    }
}
