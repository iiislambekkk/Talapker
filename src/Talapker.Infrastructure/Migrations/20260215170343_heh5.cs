using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Talapker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class heh5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LogoUrl",
                table: "faculties",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WallPaperUrl",
                table: "faculties",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LogoUrl",
                table: "faculties");

            migrationBuilder.DropColumn(
                name: "WallPaperUrl",
                table: "faculties");
        }
    }
}
