using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace LeaveRequestAPP.Migrations
{
    public partial class dateCreatedToLeaveRequest : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateCreated",
                table: "LeaveRequests",
                type: "datetime2",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateCreated",
                table: "LeaveRequests");
        }
    }
}
