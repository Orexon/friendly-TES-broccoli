using Microsoft.EntityFrameworkCore.Migrations;

namespace TES.Data.Migrations
{
    public partial class letterCasingChanges : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LastName",
                schema: "Identity",
                table: "User",
                newName: "Lastname");

            migrationBuilder.RenameColumn(
                name: "FirstName",
                schema: "Identity",
                table: "User",
                newName: "Firstname");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Lastname",
                schema: "Identity",
                table: "User",
                newName: "LastName");

            migrationBuilder.RenameColumn(
                name: "Firstname",
                schema: "Identity",
                table: "User",
                newName: "FirstName");
        }
    }
}
