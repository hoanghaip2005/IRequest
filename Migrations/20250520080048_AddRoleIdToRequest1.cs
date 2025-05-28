using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Request.Migrations
{
    /// <inheritdoc />
    public partial class AddRoleIdToRequest1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkflowSteps_Roles_RoleId",
                table: "WorkflowSteps");

            migrationBuilder.RenameColumn(
                name: "RoleId",
                table: "WorkflowSteps",
                newName: "RequiredRoleId");

            migrationBuilder.RenameIndex(
                name: "IX_WorkflowSteps_RoleId",
                table: "WorkflowSteps",
                newName: "IX_WorkflowSteps_RequiredRoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkflowSteps_Roles_RequiredRoleId",
                table: "WorkflowSteps",
                column: "RequiredRoleId",
                principalTable: "Roles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkflowSteps_Roles_RequiredRoleId",
                table: "WorkflowSteps");

            migrationBuilder.RenameColumn(
                name: "RequiredRoleId",
                table: "WorkflowSteps",
                newName: "RoleId");

            migrationBuilder.RenameIndex(
                name: "IX_WorkflowSteps_RequiredRoleId",
                table: "WorkflowSteps",
                newName: "IX_WorkflowSteps_RoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkflowSteps_Roles_RoleId",
                table: "WorkflowSteps",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id");
        }
    }
}
