using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JwtMusic.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class mig91 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "LikedSongs",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_LikedSongs_SongId",
                table: "LikedSongs",
                column: "SongId");

            migrationBuilder.AddForeignKey(
                name: "FK_LikedSongs_Songs_SongId",
                table: "LikedSongs",
                column: "SongId",
                principalTable: "Songs",
                principalColumn: "SongId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LikedSongs_Songs_SongId",
                table: "LikedSongs");

            migrationBuilder.DropIndex(
                name: "IX_LikedSongs_SongId",
                table: "LikedSongs");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "LikedSongs",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
