using Microsoft.EntityFrameworkCore.Migrations;

namespace TES.Data.Migrations
{
    public partial class ScoreBook : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PointsScored",
                schema: "Identity",
                table: "UserSolutions",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PointsScored",
                schema: "Identity",
                table: "UserSolutions");
        }
    }
}
