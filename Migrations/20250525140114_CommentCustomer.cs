using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Request.Migrations
{
    /// <inheritdoc />
    public partial class CommentCustomer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Requests_Users_AssignedUserId",
                table: "Requests");

            migrationBuilder.AlterColumn<string>(
                name: "AssignedUserId",
                table: "Requests",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "IssueType",
                table: "Requests",
                type: "nvarchar(50)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LinkedIssues",
                table: "Requests",
                type: "nvarchar(255)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Resolution",
                table: "Requests",
                type: "nvarchar(100)",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Requests_Users_AssignedUserId",
                table: "Requests",
                column: "AssignedUserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Requests_Users_AssignedUserId",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "IssueType",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "LinkedIssues",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "Resolution",
                table: "Requests");

            migrationBuilder.AlterColumn<string>(
                name: "AssignedUserId",
                table: "Requests",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Requests_Users_AssignedUserId",
                table: "Requests",
                column: "AssignedUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
