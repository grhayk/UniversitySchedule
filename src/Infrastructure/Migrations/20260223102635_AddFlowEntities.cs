using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddFlowEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupSubjectsWithLecturer_Groups_GroupId",
                table: "GroupSubjectsWithLecturer");

            migrationBuilder.DropIndex(
                name: "IX_GroupSubjectsWithLecturer_LecturerSubjectId_GroupId",
                table: "GroupSubjectsWithLecturer");

            migrationBuilder.AlterColumn<int>(
                name: "GroupId",
                table: "GroupSubjectsWithLecturer",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "FlowId",
                table: "GroupSubjectsWithLecturer",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Flows",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubjectId = table.Column<int>(type: "int", nullable: false),
                    LessonType = table.Column<byte>(type: "tinyint", nullable: false),
                    SemesterId = table.Column<int>(type: "int", nullable: false),
                    StudentsCount = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Flows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Flows_Semesters_SemesterId",
                        column: x => x.SemesterId,
                        principalTable: "Semesters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Flows_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FlowGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FlowId = table.Column<int>(type: "int", nullable: false),
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlowGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FlowGroups_Flows_FlowId",
                        column: x => x.FlowId,
                        principalTable: "Flows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FlowGroups_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GroupSubjectsWithLecturer_FlowId",
                table: "GroupSubjectsWithLecturer",
                column: "FlowId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupSubjectsWithLecturer_LecturerSubjectId_FlowId_LessonType",
                table: "GroupSubjectsWithLecturer",
                columns: new[] { "LecturerSubjectId", "FlowId", "LessonType" },
                unique: true,
                filter: "[FlowId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_GroupSubjectsWithLecturer_LecturerSubjectId_GroupId_LessonType",
                table: "GroupSubjectsWithLecturer",
                columns: new[] { "LecturerSubjectId", "GroupId", "LessonType" },
                unique: true,
                filter: "[GroupId] IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_GroupSubjectWithLecturer_GroupOrFlow",
                table: "GroupSubjectsWithLecturer",
                sql: "([GroupId] IS NOT NULL AND [FlowId] IS NULL) OR ([GroupId] IS NULL AND [FlowId] IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_FlowGroups_FlowId_GroupId",
                table: "FlowGroups",
                columns: new[] { "FlowId", "GroupId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FlowGroups_GroupId",
                table: "FlowGroups",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Flows_SemesterId",
                table: "Flows",
                column: "SemesterId");

            migrationBuilder.CreateIndex(
                name: "IX_Flows_SubjectId",
                table: "Flows",
                column: "SubjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupSubjectsWithLecturer_Flows_FlowId",
                table: "GroupSubjectsWithLecturer",
                column: "FlowId",
                principalTable: "Flows",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupSubjectsWithLecturer_Groups_GroupId",
                table: "GroupSubjectsWithLecturer",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupSubjectsWithLecturer_Flows_FlowId",
                table: "GroupSubjectsWithLecturer");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupSubjectsWithLecturer_Groups_GroupId",
                table: "GroupSubjectsWithLecturer");

            migrationBuilder.DropTable(
                name: "FlowGroups");

            migrationBuilder.DropTable(
                name: "Flows");

            migrationBuilder.DropIndex(
                name: "IX_GroupSubjectsWithLecturer_FlowId",
                table: "GroupSubjectsWithLecturer");

            migrationBuilder.DropIndex(
                name: "IX_GroupSubjectsWithLecturer_LecturerSubjectId_FlowId_LessonType",
                table: "GroupSubjectsWithLecturer");

            migrationBuilder.DropIndex(
                name: "IX_GroupSubjectsWithLecturer_LecturerSubjectId_GroupId_LessonType",
                table: "GroupSubjectsWithLecturer");

            migrationBuilder.DropCheckConstraint(
                name: "CK_GroupSubjectWithLecturer_GroupOrFlow",
                table: "GroupSubjectsWithLecturer");

            migrationBuilder.DropColumn(
                name: "FlowId",
                table: "GroupSubjectsWithLecturer");

            migrationBuilder.AlterColumn<int>(
                name: "GroupId",
                table: "GroupSubjectsWithLecturer",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GroupSubjectsWithLecturer_LecturerSubjectId_GroupId",
                table: "GroupSubjectsWithLecturer",
                columns: new[] { "LecturerSubjectId", "GroupId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupSubjectsWithLecturer_Groups_GroupId",
                table: "GroupSubjectsWithLecturer",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
