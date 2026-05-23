using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Talapker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class smthnewfs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TextContent",
                table: "KnowledgeFiles",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TextContent",
                table: "KnowledgeFiles");
        }
    }
}
