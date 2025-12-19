using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Semesters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EducationDegree = table.Column<int>(type: "int", nullable: false),
                    EducationType = table.Column<int>(type: "int", nullable: false),
                    Number = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Semesters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Structures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Type = table.Column<byte>(type: "tinyint", nullable: false),
                    FromDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ToDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Structures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Structures_Structures_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Structures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TimeTables",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StartTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeTables", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Classrooms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    StructureId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Classrooms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Classrooms_Structures_StructureId",
                        column: x => x.StructureId,
                        principalTable: "Structures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "EducationPrograms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StructureId = table.Column<int>(type: "int", nullable: false),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EducationPrograms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EducationPrograms_EducationPrograms_ParentId",
                        column: x => x.ParentId,
                        principalTable: "EducationPrograms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EducationPrograms_Structures_StructureId",
                        column: x => x.StructureId,
                        principalTable: "Structures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Subjects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SemesterIdFrom = table.Column<int>(type: "int", nullable: false),
                    SemesterIdTo = table.Column<int>(type: "int", nullable: false),
                    StructureId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subjects_Semesters_SemesterIdFrom",
                        column: x => x.SemesterIdFrom,
                        principalTable: "Semesters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Subjects_Semesters_SemesterIdTo",
                        column: x => x.SemesterIdTo,
                        principalTable: "Semesters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Subjects_Structures_StructureId",
                        column: x => x.StructureId,
                        principalTable: "Structures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClassroomCharacteristics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClassroomId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<byte>(type: "tinyint", nullable: false),
                    SeatCapacity = table.Column<int>(type: "int", nullable: false),
                    HasComputer = table.Column<bool>(type: "bit", nullable: false),
                    ComputerCount = table.Column<int>(type: "int", nullable: true),
                    HasProjector = table.Column<bool>(type: "bit", nullable: false),
                    RenovationStatus = table.Column<byte>(type: "tinyint", nullable: false),
                    BlackboardCondition = table.Column<byte>(type: "tinyint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassroomCharacteristics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClassroomCharacteristics_Classrooms_ClassroomId",
                        column: x => x.ClassroomId,
                        principalTable: "Classrooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Groups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    EducationProgramId = table.Column<int>(type: "int", nullable: false),
                    SemesterId = table.Column<int>(type: "int", nullable: false),
                    LessonType = table.Column<byte>(type: "tinyint", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IndexNumber = table.Column<int>(type: "int", nullable: false),
                    BranchedFromGroupId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Groups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Groups_EducationPrograms_EducationProgramId",
                        column: x => x.EducationProgramId,
                        principalTable: "EducationPrograms",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Groups_Groups_BranchedFromGroupId",
                        column: x => x.BranchedFromGroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Groups_Groups_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Groups_Semesters_SemesterId",
                        column: x => x.SemesterId,
                        principalTable: "Semesters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EducationProgramSubjects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EducationProgramId = table.Column<int>(type: "int", nullable: false),
                    SubjectId = table.Column<int>(type: "int", nullable: false),
                    SemesterId = table.Column<int>(type: "int", nullable: false),
                    FromDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ToDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SemesterId1 = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EducationProgramSubjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EducationProgramSubjects_EducationPrograms_EducationProgramId",
                        column: x => x.EducationProgramId,
                        principalTable: "EducationPrograms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EducationProgramSubjects_Semesters_SemesterId",
                        column: x => x.SemesterId,
                        principalTable: "Semesters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EducationProgramSubjects_Semesters_SemesterId1",
                        column: x => x.SemesterId1,
                        principalTable: "Semesters",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EducationProgramSubjects_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Person",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StructureId = table.Column<int>(type: "int", nullable: false),
                    BirthDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PersonType = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    GroupId = table.Column<int>(type: "int", nullable: true),
                    EducationDegree = table.Column<byte>(type: "tinyint", nullable: true),
                    EducationType = table.Column<byte>(type: "tinyint", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Person", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Person_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Person_Structures_StructureId",
                        column: x => x.StructureId,
                        principalTable: "Structures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Schedules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubjectId = table.Column<int>(type: "int", nullable: false),
                    TimeTableId = table.Column<int>(type: "int", nullable: false),
                    LessonTypeId = table.Column<byte>(type: "tinyint", nullable: false),
                    WeekType = table.Column<byte>(type: "tinyint", nullable: false),
                    ClassroomId = table.Column<int>(type: "int", nullable: false),
                    StaffId = table.Column<int>(type: "int", nullable: false),
                    ScheduleParentId = table.Column<int>(type: "int", nullable: true),
                    SemesterId = table.Column<int>(type: "int", nullable: false),
                    ScheduleDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Schedules_Classrooms_ClassroomId",
                        column: x => x.ClassroomId,
                        principalTable: "Classrooms",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Schedules_Person_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Person",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Schedules_Schedules_ScheduleParentId",
                        column: x => x.ScheduleParentId,
                        principalTable: "Schedules",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Schedules_Semesters_SemesterId",
                        column: x => x.SemesterId,
                        principalTable: "Semesters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Schedules_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Schedules_TimeTables_TimeTableId",
                        column: x => x.TimeTableId,
                        principalTable: "TimeTables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StaffSubjects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StaffId = table.Column<int>(type: "int", nullable: false),
                    SubjectId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffSubjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StaffSubjects_Person_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Person",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StaffSubjects_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudentGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    SemesterId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentGroups_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentGroups_Person_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Person",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentGroups_Semesters_SemesterId",
                        column: x => x.SemesterId,
                        principalTable: "Semesters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ScheduleGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScheduleId = table.Column<int>(type: "int", nullable: false),
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScheduleGroups_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ScheduleGroups_Schedules_ScheduleId",
                        column: x => x.ScheduleId,
                        principalTable: "Schedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GroupSubjectsWithStaff",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StaffSubjectId = table.Column<int>(type: "int", nullable: false),
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    Hours = table.Column<int>(type: "int", nullable: false),
                    LessonType = table.Column<byte>(type: "tinyint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupSubjectsWithStaff", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroupSubjectsWithStaff_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupSubjectsWithStaff_StaffSubjects_StaffSubjectId",
                        column: x => x.StaffSubjectId,
                        principalTable: "StaffSubjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClassroomCharacteristics_ClassroomId",
                table: "ClassroomCharacteristics",
                column: "ClassroomId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Classrooms_Name",
                table: "Classrooms",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Classrooms_StructureId",
                table: "Classrooms",
                column: "StructureId");

            migrationBuilder.CreateIndex(
                name: "IX_EducationPrograms_ParentId",
                table: "EducationPrograms",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_EducationPrograms_StructureId",
                table: "EducationPrograms",
                column: "StructureId");

            migrationBuilder.CreateIndex(
                name: "IX_EducationProgramSubjects_EducationProgramId_SubjectId_SemesterId",
                table: "EducationProgramSubjects",
                columns: new[] { "EducationProgramId", "SubjectId", "SemesterId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EducationProgramSubjects_SemesterId",
                table: "EducationProgramSubjects",
                column: "SemesterId");

            migrationBuilder.CreateIndex(
                name: "IX_EducationProgramSubjects_SemesterId1",
                table: "EducationProgramSubjects",
                column: "SemesterId1");

            migrationBuilder.CreateIndex(
                name: "IX_EducationProgramSubjects_SubjectId",
                table: "EducationProgramSubjects",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Groups_BranchedFromGroupId",
                table: "Groups",
                column: "BranchedFromGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Groups_EducationProgramId",
                table: "Groups",
                column: "EducationProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_Groups_ParentId",
                table: "Groups",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Groups_SemesterId",
                table: "Groups",
                column: "SemesterId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupSubjectsWithStaff_GroupId",
                table: "GroupSubjectsWithStaff",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupSubjectsWithStaff_StaffSubjectId_GroupId",
                table: "GroupSubjectsWithStaff",
                columns: new[] { "StaffSubjectId", "GroupId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Person_GroupId",
                table: "Person",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Person_StructureId",
                table: "Person",
                column: "StructureId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleGroups_GroupId",
                table: "ScheduleGroups",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleGroups_ScheduleId_GroupId",
                table: "ScheduleGroups",
                columns: new[] { "ScheduleId", "GroupId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_ClassroomId",
                table: "Schedules",
                column: "ClassroomId");

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_ScheduleParentId",
                table: "Schedules",
                column: "ScheduleParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_SemesterId",
                table: "Schedules",
                column: "SemesterId");

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_StaffId",
                table: "Schedules",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_SubjectId",
                table: "Schedules",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_TimeTableId",
                table: "Schedules",
                column: "TimeTableId");

            migrationBuilder.CreateIndex(
                name: "IX_Semesters_EducationDegree_EducationType_Number",
                table: "Semesters",
                columns: new[] { "EducationDegree", "EducationType", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StaffSubjects_StaffId",
                table: "StaffSubjects",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffSubjects_SubjectId",
                table: "StaffSubjects",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Structures_Code",
                table: "Structures",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Structures_ParentId",
                table: "Structures",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentGroups_GroupId",
                table: "StudentGroups",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentGroups_SemesterId",
                table: "StudentGroups",
                column: "SemesterId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentGroups_StudentId_GroupId_SemesterId",
                table: "StudentGroups",
                columns: new[] { "StudentId", "GroupId", "SemesterId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_SemesterIdFrom",
                table: "Subjects",
                column: "SemesterIdFrom");

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_SemesterIdTo",
                table: "Subjects",
                column: "SemesterIdTo");

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_StructureId",
                table: "Subjects",
                column: "StructureId");

            migrationBuilder.CreateIndex(
                name: "IX_TimeTables_StartTime_EndTime",
                table: "TimeTables",
                columns: new[] { "StartTime", "EndTime" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClassroomCharacteristics");

            migrationBuilder.DropTable(
                name: "EducationProgramSubjects");

            migrationBuilder.DropTable(
                name: "GroupSubjectsWithStaff");

            migrationBuilder.DropTable(
                name: "ScheduleGroups");

            migrationBuilder.DropTable(
                name: "StudentGroups");

            migrationBuilder.DropTable(
                name: "StaffSubjects");

            migrationBuilder.DropTable(
                name: "Schedules");

            migrationBuilder.DropTable(
                name: "Classrooms");

            migrationBuilder.DropTable(
                name: "Person");

            migrationBuilder.DropTable(
                name: "Subjects");

            migrationBuilder.DropTable(
                name: "TimeTables");

            migrationBuilder.DropTable(
                name: "Groups");

            migrationBuilder.DropTable(
                name: "EducationPrograms");

            migrationBuilder.DropTable(
                name: "Semesters");

            migrationBuilder.DropTable(
                name: "Structures");
        }
    }
}
