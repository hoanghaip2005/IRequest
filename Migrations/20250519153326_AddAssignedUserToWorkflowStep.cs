using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Request.Migrations
{
    /// <inheritdoc />
    public partial class AddAssignedUserToWorkflowStep : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AssignedUserId",
                table: "WorkflowSteps",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowSteps_AssignedUserId",
                table: "WorkflowSteps",
                column: "AssignedUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkflowSteps_Users_AssignedUserId",
                table: "WorkflowSteps",
                column: "AssignedUserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkflowSteps_Users_AssignedUserId",
                table: "WorkflowSteps");

            migrationBuilder.DropIndex(
                name: "IX_WorkflowSteps_AssignedUserId",
                table: "WorkflowSteps");

            migrationBuilder.DropColumn(
                name: "AssignedUserId",
                table: "WorkflowSteps");
        }
    }
}
