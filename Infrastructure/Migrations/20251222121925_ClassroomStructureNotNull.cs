using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class ClassroomStructureNotNull : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Classrooms_Structures_StructureId",
                table: "Classrooms");

            migrationBuilder.AlterColumn<int>(
                name: "StructureId",
                table: "Classrooms",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Classrooms_Structures_StructureId",
                table: "Classrooms",
                column: "StructureId",
                principalTable: "Structures",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Classrooms_Structures_StructureId",
                table: "Classrooms");

            migrationBuilder.AlterColumn<int>(
                name: "StructureId",
                table: "Classrooms",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Classrooms_Structures_StructureId",
                table: "Classrooms",
                column: "StructureId",
                principalTable: "Structures",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
