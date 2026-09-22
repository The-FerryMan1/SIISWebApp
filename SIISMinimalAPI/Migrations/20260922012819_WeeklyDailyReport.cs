using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIISMinimalAPI.Migrations
{
    /// <inheritdoc />
    public partial class WeeklyDailyReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Progresses_Placements_PlacementId",
                table: "Progresses");

            migrationBuilder.DropIndex(
                name: "IX_Progresses_PlacementId",
                table: "Progresses");

            migrationBuilder.CreateTable(
                name: "WeeklyReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProgressId = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeeklyReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WeeklyReports_Progresses_ProgressId",
                        column: x => x.ProgressId,
                        principalTable: "Progresses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DailyReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Date = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Activities = table.Column<string>(type: "TEXT", nullable: false),
                    Hours = table.Column<int>(type: "INTEGER", nullable: false),
                    InCharge = table.Column<string>(type: "TEXT", nullable: true),
                    WeeklyReportId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DailyReports_WeeklyReports_WeeklyReportId",
                        column: x => x.WeeklyReportId,
                        principalTable: "WeeklyReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Progresses_PlacementId",
                table: "Progresses",
                column: "PlacementId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DailyReports_WeeklyReportId",
                table: "DailyReports",
                column: "WeeklyReportId");

            migrationBuilder.CreateIndex(
                name: "IX_WeeklyReports_ProgressId",
                table: "WeeklyReports",
                column: "ProgressId");

            migrationBuilder.AddForeignKey(
                name: "FK_Progresses_Placements_PlacementId",
                table: "Progresses",
                column: "PlacementId",
                principalTable: "Placements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Progresses_Placements_PlacementId",
                table: "Progresses");

            migrationBuilder.DropTable(
                name: "DailyReports");

            migrationBuilder.DropTable(
                name: "WeeklyReports");

            migrationBuilder.DropIndex(
                name: "IX_Progresses_PlacementId",
                table: "Progresses");

            migrationBuilder.CreateIndex(
                name: "IX_Progresses_PlacementId",
                table: "Progresses",
                column: "PlacementId");

            migrationBuilder.AddForeignKey(
                name: "FK_Progresses_Placements_PlacementId",
                table: "Progresses",
                column: "PlacementId",
                principalTable: "Placements",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
