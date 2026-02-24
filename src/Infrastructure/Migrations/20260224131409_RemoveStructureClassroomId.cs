using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class RemoveStructureClassroomId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Structures_Classrooms_ClassroomId",
                table: "Structures");

            migrationBuilder.DropIndex(
                name: "IX_Structures_ClassroomId",
                table: "Structures");

            migrationBuilder.DropColumn(
                name: "ClassroomId",
                table: "Structures");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClassroomId",
                table: "Structures",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Structures_ClassroomId",
                table: "Structures",
                column: "ClassroomId");

            migrationBuilder.AddForeignKey(
                name: "FK_Structures_Classrooms_ClassroomId",
                table: "Structures",
                column: "ClassroomId",
                principalTable: "Classrooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
