using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Talapker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class heh7 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UniversityName",
                table: "Ambassadors");

            migrationBuilder.AddColumn<Guid>(
                name: "InstitutionId",
                table: "Ambassadors",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ambassadors_InstitutionId",
                table: "Ambassadors",
                column: "InstitutionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ambassadors_Institutions_InstitutionId",
                table: "Ambassadors",
                column: "InstitutionId",
                principalTable: "Institutions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ambassadors_Institutions_InstitutionId",
                table: "Ambassadors");

            migrationBuilder.DropIndex(
                name: "IX_Ambassadors_InstitutionId",
                table: "Ambassadors");

            migrationBuilder.DropColumn(
                name: "InstitutionId",
                table: "Ambassadors");

            migrationBuilder.AddColumn<string>(
                name: "UniversityName",
                table: "Ambassadors",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
