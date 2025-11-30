using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineGameStoreSystem.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTable_LikeAndDislike : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "GameId",
                table: "GameLikes",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsLike",
                table: "GameLikes",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsLike",
                table: "GameLikes");

            migrationBuilder.AlterColumn<int>(
                name: "GameId",
                table: "GameLikes",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
