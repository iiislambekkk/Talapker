using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Talapker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class heh10 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "EducationPrograms",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "MinimumUntScore",
                table: "EducationPrograms",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "WorkPlaces",
                table: "EducationPrograms",
                type: "jsonb",
                nullable: false,
                defaultValue: "{}");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Code",
                table: "EducationPrograms");

            migrationBuilder.DropColumn(
                name: "MinimumUntScore",
                table: "EducationPrograms");

            migrationBuilder.DropColumn(
                name: "WorkPlaces",
                table: "EducationPrograms");
        }
    }
}
