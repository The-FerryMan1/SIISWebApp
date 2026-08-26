using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIISMinimalAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddRequirementTypeColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MyProperty",
                table: "Progresses",
                newName: "TrainingHoursRendered");

            migrationBuilder.AddColumn<int>(
                name: "RequirementType",
                table: "Requirements",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RemainingHours",
                table: "Progresses",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TrainingHoursForWeek",
                table: "Progresses",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequirementType",
                table: "Requirements");

            migrationBuilder.DropColumn(
                name: "RemainingHours",
                table: "Progresses");

            migrationBuilder.DropColumn(
                name: "TrainingHoursForWeek",
                table: "Progresses");

            migrationBuilder.RenameColumn(
                name: "TrainingHoursRendered",
                table: "Progresses",
                newName: "MyProperty");
        }
    }
}
