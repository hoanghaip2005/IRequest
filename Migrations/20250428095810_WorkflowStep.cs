using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Request.Migrations
{
    /// <inheritdoc />
    public partial class WorkflowStep : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkflowStep_Roles_RoleId",
                table: "WorkflowStep");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkflowStep_Status_NextStatusID",
                table: "WorkflowStep");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkflowStep_Workflow_WorkflowID",
                table: "WorkflowStep");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WorkflowStep",
                table: "WorkflowStep");

            migrationBuilder.DropColumn(
                name: "AssignedToRole",
                table: "WorkflowStep");

            migrationBuilder.DropColumn(
                name: "AutoNotify",
                table: "WorkflowStep");

            migrationBuilder.RenameTable(
                name: "WorkflowStep",
                newName: "WorkflowSteps");

            migrationBuilder.RenameColumn(
                name: "NextStatusID",
                table: "WorkflowSteps",
                newName: "StatusID");

            migrationBuilder.RenameIndex(
                name: "IX_WorkflowStep_WorkflowID",
                table: "WorkflowSteps",
                newName: "IX_WorkflowSteps_WorkflowID");

            migrationBuilder.RenameIndex(
                name: "IX_WorkflowStep_RoleId",
                table: "WorkflowSteps",
                newName: "IX_WorkflowSteps_RoleId");

            migrationBuilder.RenameIndex(
                name: "IX_WorkflowStep_NextStatusID",
                table: "WorkflowSteps",
                newName: "IX_WorkflowSteps_StatusID");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Status",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "StepName",
                table: "WorkflowSteps",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AddPrimaryKey(
                name: "PK_WorkflowSteps",
                table: "WorkflowSteps",
                column: "StepID");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkflowSteps_Roles_RoleId",
                table: "WorkflowSteps",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkflowSteps_Status_StatusID",
                table: "WorkflowSteps",
                column: "StatusID",
                principalTable: "Status",
                principalColumn: "StatusID");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkflowSteps_Workflow_WorkflowID",
                table: "WorkflowSteps",
                column: "WorkflowID",
                principalTable: "Workflow",
                principalColumn: "WorkflowID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkflowSteps_Roles_RoleId",
                table: "WorkflowSteps");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkflowSteps_Status_StatusID",
                table: "WorkflowSteps");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkflowSteps_Workflow_WorkflowID",
                table: "WorkflowSteps");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WorkflowSteps",
                table: "WorkflowSteps");

            migrationBuilder.RenameTable(
                name: "WorkflowSteps",
                newName: "WorkflowStep");

            migrationBuilder.RenameColumn(
                name: "StatusID",
                table: "WorkflowStep",
                newName: "NextStatusID");

            migrationBuilder.RenameIndex(
                name: "IX_WorkflowSteps_WorkflowID",
                table: "WorkflowStep",
                newName: "IX_WorkflowStep_WorkflowID");

            migrationBuilder.RenameIndex(
                name: "IX_WorkflowSteps_StatusID",
                table: "WorkflowStep",
                newName: "IX_WorkflowStep_NextStatusID");

            migrationBuilder.RenameIndex(
                name: "IX_WorkflowSteps_RoleId",
                table: "WorkflowStep",
                newName: "IX_WorkflowStep_RoleId");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Status",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "StepName",
                table: "WorkflowStep",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<int>(
                name: "AssignedToRole",
                table: "WorkflowStep",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AutoNotify",
                table: "WorkflowStep",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_WorkflowStep",
                table: "WorkflowStep",
                column: "StepID");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkflowStep_Roles_RoleId",
                table: "WorkflowStep",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkflowStep_Status_NextStatusID",
                table: "WorkflowStep",
                column: "NextStatusID",
                principalTable: "Status",
                principalColumn: "StatusID");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkflowStep_Workflow_WorkflowID",
                table: "WorkflowStep",
                column: "WorkflowID",
                principalTable: "Workflow",
                principalColumn: "WorkflowID");
        }
    }
}
