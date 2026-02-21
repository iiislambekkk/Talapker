using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Talapker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class heh25 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "EducationPrograms");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "EducationPrograms");

            migrationBuilder.DropColumn(
                name: "PractiseBases",
                table: "EducationPrograms");

            migrationBuilder.DropColumn(
                name: "WorkPlaces",
                table: "EducationPrograms");

            migrationBuilder.AddColumn<string>(
                name: "Description_En",
                table: "EducationPrograms",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Description_Kk",
                table: "EducationPrograms",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Description_Ru",
                table: "EducationPrograms",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Disciplines",
                table: "EducationPrograms",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DurationYears",
                table: "EducationPrograms",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int[]>(
                name: "Languages",
                table: "EducationPrograms",
                type: "integer[]",
                nullable: false,
                defaultValue: new int[0]);

            migrationBuilder.AddColumn<string>(
                name: "Name_En",
                table: "EducationPrograms",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Name_Kk",
                table: "EducationPrograms",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Name_Ru",
                table: "EducationPrograms",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PractiseBases_En",
                table: "EducationPrograms",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PractiseBases_Kk",
                table: "EducationPrograms",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PractiseBases_Ru",
                table: "EducationPrograms",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Prices",
                table: "EducationPrograms",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StudyForm",
                table: "EducationPrograms",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "WorkPlaces_En",
                table: "EducationPrograms",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "WorkPlaces_Kk",
                table: "EducationPrograms",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "WorkPlaces_Ru",
                table: "EducationPrograms",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description_En",
                table: "EducationPrograms");

            migrationBuilder.DropColumn(
                name: "Description_Kk",
                table: "EducationPrograms");

            migrationBuilder.DropColumn(
                name: "Description_Ru",
                table: "EducationPrograms");

            migrationBuilder.DropColumn(
                name: "Disciplines",
                table: "EducationPrograms");

            migrationBuilder.DropColumn(
                name: "DurationYears",
                table: "EducationPrograms");

            migrationBuilder.DropColumn(
                name: "Languages",
                table: "EducationPrograms");

            migrationBuilder.DropColumn(
                name: "Name_En",
                table: "EducationPrograms");

            migrationBuilder.DropColumn(
                name: "Name_Kk",
                table: "EducationPrograms");

            migrationBuilder.DropColumn(
                name: "Name_Ru",
                table: "EducationPrograms");

            migrationBuilder.DropColumn(
                name: "PractiseBases_En",
                table: "EducationPrograms");

            migrationBuilder.DropColumn(
                name: "PractiseBases_Kk",
                table: "EducationPrograms");

            migrationBuilder.DropColumn(
                name: "PractiseBases_Ru",
                table: "EducationPrograms");

            migrationBuilder.DropColumn(
                name: "Prices",
                table: "EducationPrograms");

            migrationBuilder.DropColumn(
                name: "StudyForm",
                table: "EducationPrograms");

            migrationBuilder.DropColumn(
                name: "WorkPlaces_En",
                table: "EducationPrograms");

            migrationBuilder.DropColumn(
                name: "WorkPlaces_Kk",
                table: "EducationPrograms");

            migrationBuilder.DropColumn(
                name: "WorkPlaces_Ru",
                table: "EducationPrograms");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "EducationPrograms",
                type: "jsonb",
                nullable: false,
                defaultValue: "{}");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "EducationPrograms",
                type: "jsonb",
                nullable: false,
                defaultValue: "{}");

            migrationBuilder.AddColumn<string>(
                name: "PractiseBases",
                table: "EducationPrograms",
                type: "jsonb",
                nullable: false,
                defaultValue: "{}");

            migrationBuilder.AddColumn<string>(
                name: "WorkPlaces",
                table: "EducationPrograms",
                type: "jsonb",
                nullable: false,
                defaultValue: "{}");
        }
    }
}
