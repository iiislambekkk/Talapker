using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Talapker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class newasas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApplicationUserInstitution",
                columns: table => new
                {
                    ProspectsId = table.Column<string>(type: "text", nullable: false),
                    SubscribedInstitutionsId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationUserInstitution", x => new { x.ProspectsId, x.SubscribedInstitutionsId });
                    table.ForeignKey(
                        name: "FK_ApplicationUserInstitution_AspNetUsers_ProspectsId",
                        column: x => x.ProspectsId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApplicationUserInstitution_Institutions_SubscribedInstituti~",
                        column: x => x.SubscribedInstitutionsId,
                        principalTable: "Institutions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUserInstitution_SubscribedInstitutionsId",
                table: "ApplicationUserInstitution",
                column: "SubscribedInstitutionsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicationUserInstitution");
        }
    }
}
