using Microsoft.EntityFrameworkCore.Migrations;

namespace LeaveRequestAPP.Migrations
{
    public partial class addedTotalSickLeaveTakenToUsers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SickLeave",
                table: "AspNetUsers",
                newName: "TotalSickLeaveTaken");

            migrationBuilder.RenameColumn(
                name: "AnnualLeave",
                table: "AspNetUsers",
                newName: "TotalAnnualLeaveTaken");

            migrationBuilder.AlterColumn<int>(
                name: "NoOfDays",
                table: "LeaveRequests",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TotalSickLeaveTaken",
                table: "AspNetUsers",
                newName: "SickLeave");

            migrationBuilder.RenameColumn(
                name: "TotalAnnualLeaveTaken",
                table: "AspNetUsers",
                newName: "AnnualLeave");

            migrationBuilder.AlterColumn<string>(
                name: "NoOfDays",
                table: "LeaveRequests",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
