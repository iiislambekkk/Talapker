using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Talapker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class haahs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MinimumGrantUntScore",
                table: "EducationPrograms",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MinimumPlatnoeUntScore",
                table: "EducationPrograms",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MinimumGrantUntScore",
                table: "EducationPrograms");

            migrationBuilder.DropColumn(
                name: "MinimumPlatnoeUntScore",
                table: "EducationPrograms");
        }
    }
}
