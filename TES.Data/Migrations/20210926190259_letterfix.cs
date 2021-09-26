using Microsoft.EntityFrameworkCore.Migrations;

namespace TES.Data.Migrations
{
    public partial class letterfix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserSolutions_Applicant_AplicantIdId",
                schema: "Identity",
                table: "UserSolutions");

            migrationBuilder.RenameColumn(
                name: "AplicantIdId",
                schema: "Identity",
                table: "UserSolutions",
                newName: "ApplicantIdId");

            migrationBuilder.RenameIndex(
                name: "IX_UserSolutions_AplicantIdId",
                schema: "Identity",
                table: "UserSolutions",
                newName: "IX_UserSolutions_ApplicantIdId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserSolutions_Applicant_ApplicantIdId",
                schema: "Identity",
                table: "UserSolutions",
                column: "ApplicantIdId",
                principalSchema: "Identity",
                principalTable: "Applicant",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserSolutions_Applicant_ApplicantIdId",
                schema: "Identity",
                table: "UserSolutions");

            migrationBuilder.RenameColumn(
                name: "ApplicantIdId",
                schema: "Identity",
                table: "UserSolutions",
                newName: "AplicantIdId");

            migrationBuilder.RenameIndex(
                name: "IX_UserSolutions_ApplicantIdId",
                schema: "Identity",
                table: "UserSolutions",
                newName: "IX_UserSolutions_AplicantIdId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserSolutions_Applicant_AplicantIdId",
                schema: "Identity",
                table: "UserSolutions",
                column: "AplicantIdId",
                principalSchema: "Identity",
                principalTable: "Applicant",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
