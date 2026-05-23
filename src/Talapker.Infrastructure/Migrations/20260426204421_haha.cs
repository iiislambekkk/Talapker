using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Talapker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class haha : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UploadedById",
                table: "KnowledgeFiles",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UploadedById",
                table: "KnowledgeFiles");
        }
    }
}
