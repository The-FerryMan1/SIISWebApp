using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIISMinimalAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitInitInitInit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationModel_StudentModel_StudentId",
                table: "ApplicationModel");

            migrationBuilder.DropForeignKey(
                name: "FK_InternshipModel_StudentModel_StudentId",
                table: "InternshipModel");

            migrationBuilder.DropForeignKey(
                name: "FK_RequirementModel_StudentModel_StudentId",
                table: "RequirementModel");

            migrationBuilder.DropForeignKey(
                name: "FK_SchoolModel_StudentModel_StudentId",
                table: "SchoolModel");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentModel_OfficeModel_OfficeId",
                table: "StudentModel");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StudentModel",
                table: "StudentModel");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SchoolModel",
                table: "SchoolModel");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RequirementModel",
                table: "RequirementModel");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OfficeModel",
                table: "OfficeModel");

            migrationBuilder.DropPrimaryKey(
                name: "PK_InternshipModel",
                table: "InternshipModel");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ApplicationModel",
                table: "ApplicationModel");

            migrationBuilder.RenameTable(
                name: "StudentModel",
                newName: "Students");

            migrationBuilder.RenameTable(
                name: "SchoolModel",
                newName: "School");

            migrationBuilder.RenameTable(
                name: "RequirementModel",
                newName: "Requirements");

            migrationBuilder.RenameTable(
                name: "OfficeModel",
                newName: "Offices");

            migrationBuilder.RenameTable(
                name: "InternshipModel",
                newName: "Internship");

            migrationBuilder.RenameTable(
                name: "ApplicationModel",
                newName: "Applications");

            migrationBuilder.RenameIndex(
                name: "IX_StudentModel_StudentUUID",
                table: "Students",
                newName: "IX_Students_StudentUUID");

            migrationBuilder.RenameIndex(
                name: "IX_StudentModel_OfficeId",
                table: "Students",
                newName: "IX_Students_OfficeId");

            migrationBuilder.RenameIndex(
                name: "IX_SchoolModel_StudentId",
                table: "School",
                newName: "IX_School_StudentId");

            migrationBuilder.RenameIndex(
                name: "IX_RequirementModel_StudentId",
                table: "Requirements",
                newName: "IX_Requirements_StudentId");

            migrationBuilder.RenameIndex(
                name: "IX_InternshipModel_StudentId",
                table: "Internship",
                newName: "IX_Internship_StudentId");

            migrationBuilder.RenameIndex(
                name: "IX_ApplicationModel_StudentId",
                table: "Applications",
                newName: "IX_Applications_StudentId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Students",
                table: "Students",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_School",
                table: "School",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Requirements",
                table: "Requirements",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Offices",
                table: "Offices",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Internship",
                table: "Internship",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Applications",
                table: "Applications",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Applications_Students_StudentId",
                table: "Applications",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Internship_Students_StudentId",
                table: "Internship",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Requirements_Students_StudentId",
                table: "Requirements",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_School_Students_StudentId",
                table: "School",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Students_Offices_OfficeId",
                table: "Students",
                column: "OfficeId",
                principalTable: "Offices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Applications_Students_StudentId",
                table: "Applications");

            migrationBuilder.DropForeignKey(
                name: "FK_Internship_Students_StudentId",
                table: "Internship");

            migrationBuilder.DropForeignKey(
                name: "FK_Requirements_Students_StudentId",
                table: "Requirements");

            migrationBuilder.DropForeignKey(
                name: "FK_School_Students_StudentId",
                table: "School");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_Offices_OfficeId",
                table: "Students");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Students",
                table: "Students");

            migrationBuilder.DropPrimaryKey(
                name: "PK_School",
                table: "School");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Requirements",
                table: "Requirements");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Offices",
                table: "Offices");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Internship",
                table: "Internship");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Applications",
                table: "Applications");

            migrationBuilder.RenameTable(
                name: "Students",
                newName: "StudentModel");

            migrationBuilder.RenameTable(
                name: "School",
                newName: "SchoolModel");

            migrationBuilder.RenameTable(
                name: "Requirements",
                newName: "RequirementModel");

            migrationBuilder.RenameTable(
                name: "Offices",
                newName: "OfficeModel");

            migrationBuilder.RenameTable(
                name: "Internship",
                newName: "InternshipModel");

            migrationBuilder.RenameTable(
                name: "Applications",
                newName: "ApplicationModel");

            migrationBuilder.RenameIndex(
                name: "IX_Students_StudentUUID",
                table: "StudentModel",
                newName: "IX_StudentModel_StudentUUID");

            migrationBuilder.RenameIndex(
                name: "IX_Students_OfficeId",
                table: "StudentModel",
                newName: "IX_StudentModel_OfficeId");

            migrationBuilder.RenameIndex(
                name: "IX_School_StudentId",
                table: "SchoolModel",
                newName: "IX_SchoolModel_StudentId");

            migrationBuilder.RenameIndex(
                name: "IX_Requirements_StudentId",
                table: "RequirementModel",
                newName: "IX_RequirementModel_StudentId");

            migrationBuilder.RenameIndex(
                name: "IX_Internship_StudentId",
                table: "InternshipModel",
                newName: "IX_InternshipModel_StudentId");

            migrationBuilder.RenameIndex(
                name: "IX_Applications_StudentId",
                table: "ApplicationModel",
                newName: "IX_ApplicationModel_StudentId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StudentModel",
                table: "StudentModel",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SchoolModel",
                table: "SchoolModel",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RequirementModel",
                table: "RequirementModel",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OfficeModel",
                table: "OfficeModel",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_InternshipModel",
                table: "InternshipModel",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ApplicationModel",
                table: "ApplicationModel",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationModel_StudentModel_StudentId",
                table: "ApplicationModel",
                column: "StudentId",
                principalTable: "StudentModel",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InternshipModel_StudentModel_StudentId",
                table: "InternshipModel",
                column: "StudentId",
                principalTable: "StudentModel",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RequirementModel_StudentModel_StudentId",
                table: "RequirementModel",
                column: "StudentId",
                principalTable: "StudentModel",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SchoolModel_StudentModel_StudentId",
                table: "SchoolModel",
                column: "StudentId",
                principalTable: "StudentModel",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentModel_OfficeModel_OfficeId",
                table: "StudentModel",
                column: "OfficeId",
                principalTable: "OfficeModel",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
