using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Talapker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class heh2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Major",
                table: "Ambassadors");

            migrationBuilder.AddColumn<Guid>(
                name: "EducationalProgramId",
                table: "Ambassadors",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ambassadors_EducationalProgramId",
                table: "Ambassadors",
                column: "EducationalProgramId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ambassadors_EducationPrograms_EducationalProgramId",
                table: "Ambassadors",
                column: "EducationalProgramId",
                principalTable: "EducationPrograms",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ambassadors_EducationPrograms_EducationalProgramId",
                table: "Ambassadors");

            migrationBuilder.DropIndex(
                name: "IX_Ambassadors_EducationalProgramId",
                table: "Ambassadors");

            migrationBuilder.DropColumn(
                name: "EducationalProgramId",
                table: "Ambassadors");

            migrationBuilder.AddColumn<string>(
                name: "Major",
                table: "Ambassadors",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
