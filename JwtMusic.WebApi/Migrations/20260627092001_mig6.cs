using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JwtMusic.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class mig6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPremium",
                table: "Songs");

            migrationBuilder.AddColumn<string>(
                name: "RequiredRole",
                table: "Songs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequiredRole",
                table: "Songs");

            migrationBuilder.AddColumn<bool>(
                name: "IsPremium",
                table: "Songs",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
