using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PfeManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SwitchUserInheritanceToTpt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Meetings_Users_CreatedById",
                table: "Meetings");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Projects_ProjectId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Users_CompSupervisorId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Users_UniSupervisorId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_CompSupervisorId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_ProjectId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_UniSupervisorId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "BadgeIMG",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Cin",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CompSupervisorId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CompanyName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CompanySupervisor_BadgeIMG",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CompanySupervisor_CompanyName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Degree",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DegreeType",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "StudentIdCardIMG",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UniSupervisorId",
                table: "Users");

            migrationBuilder.CreateTable(
                name: "CompanySupervisors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyName = table.Column<string>(type: "text", nullable: false),
                    BadgeIMG = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanySupervisors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanySupervisors_Users_Id",
                        column: x => x.Id,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UniversitySupervisors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BadgeIMG = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UniversitySupervisors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UniversitySupervisors_Users_Id",
                        column: x => x.Id,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Students",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Cin = table.Column<string>(type: "text", nullable: false),
                    StudentIdCardIMG = table.Column<string>(type: "text", nullable: false),
                    CompanyName = table.Column<string>(type: "text", nullable: false),
                    Degree = table.Column<int>(type: "integer", nullable: false),
                    DegreeType = table.Column<int>(type: "integer", nullable: false),
                    CompSupervisorId = table.Column<Guid>(type: "uuid", nullable: false),
                    UniSupervisorId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Students_CompanySupervisors_CompSupervisorId",
                        column: x => x.CompSupervisorId,
                        principalTable: "CompanySupervisors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Students_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Students_UniversitySupervisors_UniSupervisorId",
                        column: x => x.UniSupervisorId,
                        principalTable: "UniversitySupervisors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Students_Users_Id",
                        column: x => x.Id,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Students_Cin",
                table: "Students",
                column: "Cin",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Students_CompSupervisorId",
                table: "Students",
                column: "CompSupervisorId");

            migrationBuilder.CreateIndex(
                name: "IX_Students_ProjectId",
                table: "Students",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Students_UniSupervisorId",
                table: "Students",
                column: "UniSupervisorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Meetings_Students_CreatedById",
                table: "Meetings",
                column: "CreatedById",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Meetings_Students_CreatedById",
                table: "Meetings");

            migrationBuilder.DropTable(
                name: "Students");

            migrationBuilder.DropTable(
                name: "CompanySupervisors");

            migrationBuilder.DropTable(
                name: "UniversitySupervisors");

            migrationBuilder.AddColumn<string>(
                name: "BadgeIMG",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Cin",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CompSupervisorId",
                table: "Users",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyName",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanySupervisor_BadgeIMG",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanySupervisor_CompanyName",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Degree",
                table: "Users",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DegreeType",
                table: "Users",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProjectId",
                table: "Users",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StudentIdCardIMG",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UniSupervisorId",
                table: "Users",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_CompSupervisorId",
                table: "Users",
                column: "CompSupervisorId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_ProjectId",
                table: "Users",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_UniSupervisorId",
                table: "Users",
                column: "UniSupervisorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Meetings_Users_CreatedById",
                table: "Meetings",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Projects_ProjectId",
                table: "Users",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Users_CompSupervisorId",
                table: "Users",
                column: "CompSupervisorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Users_UniSupervisorId",
                table: "Users",
                column: "UniSupervisorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
