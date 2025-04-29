using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Request.Migrations
{
    /// <inheritdoc />
    public partial class Pri_Workflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SortOrder",
                table: "Priority");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                table: "Priority",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
