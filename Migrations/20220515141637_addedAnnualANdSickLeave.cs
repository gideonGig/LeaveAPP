using Microsoft.EntityFrameworkCore.Migrations;

namespace LeaveRequestAPP.Migrations
{
    public partial class addedAnnualANdSickLeave : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AnnualLeave",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 21);

            migrationBuilder.AddColumn<int>(
                name: "SickLeave",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 5);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AnnualLeave",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "SickLeave",
                table: "AspNetUsers");
        }
    }
}
