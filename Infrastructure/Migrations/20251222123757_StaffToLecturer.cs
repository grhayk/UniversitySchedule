using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class StaffToLecturer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupSubjectsWithLecturer_StaffSubjects_LecturerSubjectId",
                table: "GroupSubjectsWithLecturer");

            migrationBuilder.DropForeignKey(
                name: "FK_StaffSubjects_Person_LecturerId",
                table: "StaffSubjects");

            migrationBuilder.DropForeignKey(
                name: "FK_StaffSubjects_Subjects_SubjectId",
                table: "StaffSubjects");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StaffSubjects",
                table: "StaffSubjects");

            migrationBuilder.RenameTable(
                name: "StaffSubjects",
                newName: "LecturerSubjects");

            migrationBuilder.RenameIndex(
                name: "IX_StaffSubjects_SubjectId",
                table: "LecturerSubjects",
                newName: "IX_LecturerSubjects_SubjectId");

            migrationBuilder.RenameIndex(
                name: "IX_StaffSubjects_LecturerId",
                table: "LecturerSubjects",
                newName: "IX_LecturerSubjects_LecturerId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LecturerSubjects",
                table: "LecturerSubjects",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupSubjectsWithLecturer_LecturerSubjects_LecturerSubjectId",
                table: "GroupSubjectsWithLecturer",
                column: "LecturerSubjectId",
                principalTable: "LecturerSubjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LecturerSubjects_Person_LecturerId",
                table: "LecturerSubjects",
                column: "LecturerId",
                principalTable: "Person",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LecturerSubjects_Subjects_SubjectId",
                table: "LecturerSubjects",
                column: "SubjectId",
                principalTable: "Subjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupSubjectsWithLecturer_LecturerSubjects_LecturerSubjectId",
                table: "GroupSubjectsWithLecturer");

            migrationBuilder.DropForeignKey(
                name: "FK_LecturerSubjects_Person_LecturerId",
                table: "LecturerSubjects");

            migrationBuilder.DropForeignKey(
                name: "FK_LecturerSubjects_Subjects_SubjectId",
                table: "LecturerSubjects");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LecturerSubjects",
                table: "LecturerSubjects");

            migrationBuilder.RenameTable(
                name: "LecturerSubjects",
                newName: "StaffSubjects");

            migrationBuilder.RenameIndex(
                name: "IX_LecturerSubjects_SubjectId",
                table: "StaffSubjects",
                newName: "IX_StaffSubjects_SubjectId");

            migrationBuilder.RenameIndex(
                name: "IX_LecturerSubjects_LecturerId",
                table: "StaffSubjects",
                newName: "IX_StaffSubjects_LecturerId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StaffSubjects",
                table: "StaffSubjects",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupSubjectsWithLecturer_StaffSubjects_LecturerSubjectId",
                table: "GroupSubjectsWithLecturer",
                column: "LecturerSubjectId",
                principalTable: "StaffSubjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StaffSubjects_Person_LecturerId",
                table: "StaffSubjects",
                column: "LecturerId",
                principalTable: "Person",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StaffSubjects_Subjects_SubjectId",
                table: "StaffSubjects",
                column: "SubjectId",
                principalTable: "Subjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
