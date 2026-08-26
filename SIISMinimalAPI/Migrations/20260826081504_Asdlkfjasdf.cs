using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIISMinimalAPI.Migrations
{
    /// <inheritdoc />
    public partial class Asdlkfjasdf : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlacementStatus",
                table: "Progresses");

            migrationBuilder.AddColumn<int>(
                name: "PlacementStatus",
                table: "Placements",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlacementStatus",
                table: "Placements");

            migrationBuilder.AddColumn<int>(
                name: "PlacementStatus",
                table: "Progresses",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }
    }
}
