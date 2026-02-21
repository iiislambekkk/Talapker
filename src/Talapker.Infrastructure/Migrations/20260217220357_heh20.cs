using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Talapker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class heh20 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "InstitutionId",
                table: "AssistantChatMessage",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_AssistantChatMessage_InstitutionId",
                table: "AssistantChatMessage",
                column: "InstitutionId");

            migrationBuilder.AddForeignKey(
                name: "FK_AssistantChatMessage_Institutions_InstitutionId",
                table: "AssistantChatMessage",
                column: "InstitutionId",
                principalTable: "Institutions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssistantChatMessage_Institutions_InstitutionId",
                table: "AssistantChatMessage");

            migrationBuilder.DropIndex(
                name: "IX_AssistantChatMessage_InstitutionId",
                table: "AssistantChatMessage");

            migrationBuilder.DropColumn(
                name: "InstitutionId",
                table: "AssistantChatMessage");
        }
    }
}
