using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineGameStoreSystem.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTable_GameStorageRequireMB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StorageRequireMB",
                table: "Games",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StorageRequireMB",
                table: "Games");
        }
    }
}
