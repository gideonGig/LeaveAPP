using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace LeaveRequestAPP.Data
{
    public enum RoleType
    {
        Manager,
        User,
        HumanResource,
        SoftwareEngineer,
        PetroleumEngineer,
        Accountant,
        BusinessDeveloment,
        Administration
    }
    public class LeaveRequest
    {
        [Key]
        public string Id { get; set; }
        public string EmployeeId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? DateCreated { get; set; }
        public string LeaveType { get; set; }
        public string ApprovedBy { get; set; }
        public string HrAdmin { get; set; }
        public string Comment { get; set; }
        public string Status { get; set; }
        public int NoOfDays { get; set; }
        public bool IsCalenderEventCreated { get; set; }
    }

    public class Employee : IdentityUser
    {  
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmployeeId { get; set; }
        public string JobRole { get; set; }
        public int RemainingSickLeave { get; set; }
        public int RemainingAnnualLeave { get; set; }
        public int AnnualLeave { get; set; } 
        public int SickLeave { get; set; } 
        public int  TotalAnnualLeaveTaken { get; set; }
        public int TotalSickLeaveTaken { get; set; }
        public string UserType { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime? DateTokenReceived { get; set; }
    }

    public class ApplicationRole : IdentityRole 
    {
        public ApplicationRole()
        {

        }
        public string Description { get; set; }
        public string Status { get; set; }
        public ApplicationRole(string name, string description, string status) : base(name)
        {
            this.Description = description;
            this.Status = status;
        }
    }

    public class AppToken
    {
        [Key]
        public int Id { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public int ExpireTime { get; set; }
        public DateTime? DateReceived { get; set; }
        public string Scope { get; set; }
        public string TokenType { get; set; }
    }

}
