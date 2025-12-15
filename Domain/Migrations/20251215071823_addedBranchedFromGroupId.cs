using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class addedBranchedFromGroupId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Status",
                table: "ClassroomCharacteristics",
                newName: "Type");

            migrationBuilder.AddColumn<int>(
                name: "BranchedFromGroupId",
                table: "Groups",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Groups_BranchedFromGroupId",
                table: "Groups",
                column: "BranchedFromGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_Groups_Groups_BranchedFromGroupId",
                table: "Groups",
                column: "BranchedFromGroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Groups_Groups_BranchedFromGroupId",
                table: "Groups");

            migrationBuilder.DropIndex(
                name: "IX_Groups_BranchedFromGroupId",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "BranchedFromGroupId",
                table: "Groups");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "ClassroomCharacteristics",
                newName: "Status");
        }
    }
}
