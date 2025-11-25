using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineGameStoreSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddOtpEntries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeveloperRevenue_Games_GameId",
                table: "DeveloperRevenue");

            migrationBuilder.DropForeignKey(
                name: "FK_DeveloperRevenue_Purchases_PurchaseId",
                table: "DeveloperRevenue");

            migrationBuilder.DropForeignKey(
                name: "FK_DeveloperRevenue_Users_DeveloperId",
                table: "DeveloperRevenue");

            migrationBuilder.DropForeignKey(
                name: "FK_OtpEntry_Users_UserId",
                table: "OtpEntry");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OtpEntry",
                table: "OtpEntry");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DeveloperRevenue",
                table: "DeveloperRevenue");

            migrationBuilder.RenameTable(
                name: "OtpEntry",
                newName: "OtpEntries");

            migrationBuilder.RenameTable(
                name: "DeveloperRevenue",
                newName: "DeveloperRevenues");

            migrationBuilder.RenameIndex(
                name: "IX_OtpEntry_UserId",
                table: "OtpEntries",
                newName: "IX_OtpEntries_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_DeveloperRevenue_PurchaseId",
                table: "DeveloperRevenues",
                newName: "IX_DeveloperRevenues_PurchaseId");

            migrationBuilder.RenameIndex(
                name: "IX_DeveloperRevenue_GameId",
                table: "DeveloperRevenues",
                newName: "IX_DeveloperRevenues_GameId");

            migrationBuilder.RenameIndex(
                name: "IX_DeveloperRevenue_DeveloperId",
                table: "DeveloperRevenues",
                newName: "IX_DeveloperRevenues_DeveloperId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OtpEntries",
                table: "OtpEntries",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DeveloperRevenues",
                table: "DeveloperRevenues",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DeveloperRevenues_Games_GameId",
                table: "DeveloperRevenues",
                column: "GameId",
                principalTable: "Games",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DeveloperRevenues_Purchases_PurchaseId",
                table: "DeveloperRevenues",
                column: "PurchaseId",
                principalTable: "Purchases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DeveloperRevenues_Users_DeveloperId",
                table: "DeveloperRevenues",
                column: "DeveloperId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OtpEntries_Users_UserId",
                table: "OtpEntries",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeveloperRevenues_Games_GameId",
                table: "DeveloperRevenues");

            migrationBuilder.DropForeignKey(
                name: "FK_DeveloperRevenues_Purchases_PurchaseId",
                table: "DeveloperRevenues");

            migrationBuilder.DropForeignKey(
                name: "FK_DeveloperRevenues_Users_DeveloperId",
                table: "DeveloperRevenues");

            migrationBuilder.DropForeignKey(
                name: "FK_OtpEntries_Users_UserId",
                table: "OtpEntries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OtpEntries",
                table: "OtpEntries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DeveloperRevenues",
                table: "DeveloperRevenues");

            migrationBuilder.RenameTable(
                name: "OtpEntries",
                newName: "OtpEntry");

            migrationBuilder.RenameTable(
                name: "DeveloperRevenues",
                newName: "DeveloperRevenue");

            migrationBuilder.RenameIndex(
                name: "IX_OtpEntries_UserId",
                table: "OtpEntry",
                newName: "IX_OtpEntry_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_DeveloperRevenues_PurchaseId",
                table: "DeveloperRevenue",
                newName: "IX_DeveloperRevenue_PurchaseId");

            migrationBuilder.RenameIndex(
                name: "IX_DeveloperRevenues_GameId",
                table: "DeveloperRevenue",
                newName: "IX_DeveloperRevenue_GameId");

            migrationBuilder.RenameIndex(
                name: "IX_DeveloperRevenues_DeveloperId",
                table: "DeveloperRevenue",
                newName: "IX_DeveloperRevenue_DeveloperId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OtpEntry",
                table: "OtpEntry",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DeveloperRevenue",
                table: "DeveloperRevenue",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DeveloperRevenue_Games_GameId",
                table: "DeveloperRevenue",
                column: "GameId",
                principalTable: "Games",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DeveloperRevenue_Purchases_PurchaseId",
                table: "DeveloperRevenue",
                column: "PurchaseId",
                principalTable: "Purchases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DeveloperRevenue_Users_DeveloperId",
                table: "DeveloperRevenue",
                column: "DeveloperId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OtpEntry_Users_UserId",
                table: "OtpEntry",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
