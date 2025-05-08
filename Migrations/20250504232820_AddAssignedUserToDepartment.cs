using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Request.Migrations
{
    /// <inheritdoc />
    public partial class AddAssignedUserToDepartment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AssignedUserId",
                table: "Requests",
                type: "nvarchar(450)",
                nullable: true,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_AssignedUserId",
                table: "Requests",
                column: "AssignedUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Requests_Users_AssignedUserId",
                table: "Requests",
                column: "AssignedUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddColumn<string>(
                name: "AssignedUserId",
                table: "Departments",
                type: "nvarchar(450)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Requests_Users_AssignedUserId",
                table: "Requests");

            migrationBuilder.DropIndex(
                name: "IX_Requests_AssignedUserId",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "AssignedUserId",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "AssignedUserId",
                table: "Departments");
        }
    }
}
