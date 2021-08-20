using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TES.Data.Migrations
{
    public partial class ScoreBook1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Overtime",
                schema: "Identity",
                table: "Results");

            migrationBuilder.AlterColumn<double>(
                name: "PointsScored",
                schema: "Identity",
                table: "UserSolutions",
                type: "float",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<double>(
                name: "TotalPoints",
                schema: "Identity",
                table: "Results",
                type: "float",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "MinutesOvertime",
                schema: "Identity",
                table: "Results",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MinutesOvertime",
                schema: "Identity",
                table: "Results");

            migrationBuilder.AlterColumn<int>(
                name: "PointsScored",
                schema: "Identity",
                table: "UserSolutions",
                type: "int",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<int>(
                name: "TotalPoints",
                schema: "Identity",
                table: "Results",
                type: "int",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "Overtime",
                schema: "Identity",
                table: "Results",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));
        }
    }
}
