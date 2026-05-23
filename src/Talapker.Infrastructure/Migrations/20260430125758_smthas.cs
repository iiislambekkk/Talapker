using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Talapker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class smthas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "bot_tokens",
                columns: table => new
                {
                    bot_id = table.Column<Guid>(type: "uuid", nullable: false),
                    encrypted_token = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bot_tokens", x => x.bot_id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bot_tokens");
        }
    }
}
