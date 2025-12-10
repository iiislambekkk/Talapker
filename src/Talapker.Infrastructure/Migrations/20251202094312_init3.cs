using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Talapker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class init3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EducationDirections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Degree = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EducationDirections", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "faculties",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "jsonb", nullable: false),
                    InstitutionId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_faculties", x => x.Id);
                    table.ForeignKey(
                        name: "FK_faculties_Institutions_InstitutionId",
                        column: x => x.InstitutionId,
                        principalTable: "Institutions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UntSubjects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SeedId = table.Column<int>(type: "integer", nullable: true),
                    description = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UntSubjects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EducationFields",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NationalCode = table.Column<string>(type: "text", nullable: false),
                    EducationDirectionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EducationFields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EducationFields_EducationDirections_EducationDirectionId",
                        column: x => x.EducationDirectionId,
                        principalTable: "EducationDirections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UntPairs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SeedId = table.Column<int>(type: "integer", nullable: true),
                    FirstSubjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    SecondSubjectId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UntPairs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UntPairs_UntSubjects_FirstSubjectId",
                        column: x => x.FirstSubjectId,
                        principalTable: "UntSubjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UntPairs_UntSubjects_SecondSubjectId",
                        column: x => x.SecondSubjectId,
                        principalTable: "UntSubjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EducationGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NationalCode = table.Column<string>(type: "text", nullable: false),
                    EducationFieldId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EducationGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EducationGroups_EducationFields_EducationFieldId",
                        column: x => x.EducationFieldId,
                        principalTable: "EducationFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EducationGroupUntPair",
                columns: table => new
                {
                    EducationGroupsId = table.Column<Guid>(type: "uuid", nullable: false),
                    UntSubjectsPairsId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EducationGroupUntPair", x => new { x.EducationGroupsId, x.UntSubjectsPairsId });
                    table.ForeignKey(
                        name: "FK_EducationGroupUntPair_EducationGroups_EducationGroupsId",
                        column: x => x.EducationGroupsId,
                        principalTable: "EducationGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EducationGroupUntPair_UntPairs_UntSubjectsPairsId",
                        column: x => x.UntSubjectsPairsId,
                        principalTable: "UntPairs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EducationPrograms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FacultyId = table.Column<Guid>(type: "uuid", nullable: false),
                    EducationGroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "jsonb", nullable: false),
                    Name = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EducationPrograms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EducationPrograms_EducationGroups_EducationGroupId",
                        column: x => x.EducationGroupId,
                        principalTable: "EducationGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EducationPrograms_faculties_FacultyId",
                        column: x => x.FacultyId,
                        principalTable: "faculties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GrantCompetitionStatistics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    CompetitionType = table.Column<int>(type: "integer", nullable: false),
                    EducationGroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    MinScore = table.Column<int>(type: "integer", nullable: false),
                    Records = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrantCompetitionStatistics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GrantCompetitionStatistics_EducationGroups_EducationGroupId",
                        column: x => x.EducationGroupId,
                        principalTable: "EducationGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EducationFields_EducationDirectionId",
                table: "EducationFields",
                column: "EducationDirectionId");

            migrationBuilder.CreateIndex(
                name: "IX_EducationFields_NationalCode",
                table: "EducationFields",
                column: "NationalCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EducationGroups_EducationFieldId",
                table: "EducationGroups",
                column: "EducationFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_EducationGroups_NationalCode",
                table: "EducationGroups",
                column: "NationalCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EducationGroupUntPair_UntSubjectsPairsId",
                table: "EducationGroupUntPair",
                column: "UntSubjectsPairsId");

            migrationBuilder.CreateIndex(
                name: "IX_EducationPrograms_EducationGroupId",
                table: "EducationPrograms",
                column: "EducationGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_EducationPrograms_FacultyId",
                table: "EducationPrograms",
                column: "FacultyId");

            migrationBuilder.CreateIndex(
                name: "IX_faculties_InstitutionId",
                table: "faculties",
                column: "InstitutionId");

            migrationBuilder.CreateIndex(
                name: "IX_GrantCompetitionStatistics_EducationGroupId",
                table: "GrantCompetitionStatistics",
                column: "EducationGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_GrantCompetitionStatistics_Year_EducationGroupId_Competitio~",
                table: "GrantCompetitionStatistics",
                columns: new[] { "Year", "EducationGroupId", "CompetitionType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UntPairs_FirstSubjectId",
                table: "UntPairs",
                column: "FirstSubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_UntPairs_Id",
                table: "UntPairs",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UntPairs_SecondSubjectId",
                table: "UntPairs",
                column: "SecondSubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_UntSubjects_Id",
                table: "UntSubjects",
                column: "Id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EducationGroupUntPair");

            migrationBuilder.DropTable(
                name: "EducationPrograms");

            migrationBuilder.DropTable(
                name: "GrantCompetitionStatistics");

            migrationBuilder.DropTable(
                name: "UntPairs");

            migrationBuilder.DropTable(
                name: "faculties");

            migrationBuilder.DropTable(
                name: "EducationGroups");

            migrationBuilder.DropTable(
                name: "UntSubjects");

            migrationBuilder.DropTable(
                name: "EducationFields");

            migrationBuilder.DropTable(
                name: "EducationDirections");
        }
    }
}
