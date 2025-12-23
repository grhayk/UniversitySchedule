using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class fixRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EducationProgramSubjects_Semesters_SemesterId1",
                table: "EducationProgramSubjects");

            migrationBuilder.DropIndex(
                name: "IX_EducationProgramSubjects_SemesterId1",
                table: "EducationProgramSubjects");

            migrationBuilder.DropColumn(
                name: "SemesterId1",
                table: "EducationProgramSubjects");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SemesterId1",
                table: "EducationProgramSubjects",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EducationProgramSubjects_SemesterId1",
                table: "EducationProgramSubjects",
                column: "SemesterId1");

            migrationBuilder.AddForeignKey(
                name: "FK_EducationProgramSubjects_Semesters_SemesterId1",
                table: "EducationProgramSubjects",
                column: "SemesterId1",
                principalTable: "Semesters",
                principalColumn: "Id");
        }
    }
}
