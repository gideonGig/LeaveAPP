using LeaveRequestAPP.Data;
using LeaveRequestAPP.Interfaces;
using LeaveRequestAPP.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LeaveRequestAPP.Service
{
    public class ManagerLeave : IManagerLeave
    {
        private readonly ApplicationDbContext _context;
        private readonly Utility _util;
        public ManagerLeave(ApplicationDbContext context, Utility util)
        {
            _context = context;
            _util = util;
        }
        public async Task<ApiResponse> ApproveRequest(string leaverequestId, string managerEmail)
        {
            var manager = await _context.Users.FirstOrDefaultAsync(x => x.Email == managerEmail && x.UserType == RoleType.Manager.ToString());
            if (manager == null)
            {
                return ReturnedResponse.ErrorResponse("this user do not have right to approve request", null);
            }
            var lr = await _context.LeaveRequests.FirstOrDefaultAsync(x => x.Id == leaverequestId);
            if (lr == null)
            {
                return ReturnedResponse.ErrorResponse("This Leave Request Does not exist", null);
            }
            if (lr.Status == LeaveStatus.Approved.ToString())
            {
                return ReturnedResponse.ErrorResponse("This Leave Status has already been Approved", null);
            }

            var employee = await _context.Users.FirstOrDefaultAsync(x => x.EmployeeId == lr.EmployeeId);
            if (employee == null)
            {
                return ReturnedResponse.ErrorResponse("This Employee does not exist", null);
            }
            if (lr.LeaveType == LeaveType.AnnualLeave.ToString())
            {
                employee.TotalAnnualLeaveTaken += lr.NoOfDays;
                employee.RemainingAnnualLeave = employee.AnnualLeave - employee.TotalAnnualLeaveTaken;
            }
            
            if (lr.LeaveType == LeaveType.SickLeave.ToString())
            {
                //If the sick leave is less than zero, deduct it from the annual Leave
                if (employee.RemainingSickLeave <= 0)
                {
                    employee.TotalAnnualLeaveTaken += lr.NoOfDays;
                    employee.RemainingAnnualLeave = employee.AnnualLeave - employee.TotalAnnualLeaveTaken;
                }
                else
                {
                    employee.TotalSickLeaveTaken += lr.NoOfDays;
                    employee.RemainingSickLeave = employee.SickLeave - employee.TotalSickLeaveTaken;
                }           
            }
            //update the Status of the Leave to Approve
            lr.Status = LeaveStatus.Approved.ToString();

            //ADD IT TO OUTLOOK CALENDER
            try
            {
                var cEvent = new CalenderEvent();
                cEvent.allowNewTimeProposals = true;
                cEvent.start = new EventDate { dateTime = _util.Truncate(lr.StartDate.Value, TimeSpan.FromSeconds(1)), timeZone = "Africa/Lagos" };
                cEvent.end = new EventDate { dateTime = _util.Truncate(lr.EndDate.Value, TimeSpan.FromSeconds(1)), timeZone = "Africa/Lagos" };
                cEvent.body = new Body { content = $"{employee.FirstName} {employee.LastName} {lr.LeaveType} for {lr.NoOfDays} days", contentType = "HTML" };
                cEvent.location = new Location { displayName = "Accrete" };
               
                //Create the Requester has Attendee, so he can get a mail
                var attendeeEmail = new EmailAddress { address = lr.EmployeeId, name = $"{employee.FirstName} {employee.LastName}" };
                var attendee = new Attendee { emailAddress = attendeeEmail, type = "required" };

                //Create the HR has attendedd, so she can get a mail
                var listAttendees = new List<Attendee>();
                var hr = await _context.Users.FirstOrDefaultAsync(x => x.Email == lr.HrAdmin);
                if (hr != null)
                {
                    var hrEmail = new EmailAddress { address = lr.HrAdmin, name = $"{hr.FirstName} {hr.LastName}" };
                    var hrAttendee = new Attendee { emailAddress = hrEmail, type = "required" };
                    listAttendees.Add(hrAttendee);

                }
                
                listAttendees.Add(attendee);
                cEvent.attendees = listAttendees;
                
                cEvent.subject = $"{lr.EmployeeId} {lr.LeaveType}";

                var isCreated = await _util.CreateCalenderEvent(managerEmail, cEvent);
                lr.IsCalenderEventCreated = isCreated;
            }
            catch
            {

            }

            _context.Entry(lr).State = EntityState.Modified;
            _context.Entry(employee).State = EntityState.Modified;
           
            await _context.SaveChangesAsync();

            return ReturnedResponse.SuccessResponse(null, "Approved Successfully");
        }
        public async Task<ApiResponse> RejectRequest(string leaverequestId, string managerEmail)
        {
            var manager = _context.Users.FirstOrDefaultAsync(x => x.Email == managerEmail && x.UserType == RoleType.Manager.ToString());
            if (manager == null)
            {
                return ReturnedResponse.ErrorResponse("this user do not have right to approve request", null);
            }
            var lr = await _context.LeaveRequests.FirstOrDefaultAsync(x => x.Id == leaverequestId);
            if (lr == null)
            {
                return ReturnedResponse.ErrorResponse("This Leave Request Does not exist", null);
            }
            if (lr.Status == LeaveStatus.Approved.ToString())
            {
                return ReturnedResponse.ErrorResponse("This Leave Status has already been Approved", null);
            }
            //update the Status of the Leave to Approve
            lr.Status = LeaveStatus.Rejected.ToString();
            _context.Entry(lr).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return ReturnedResponse.SuccessResponse(null, "Rejected Successfully");
        }
        public async Task<ApiResponse> DeleteRequest(string leaverequestId)
        {
            var lr = await _context.LeaveRequests.FirstOrDefaultAsync(x => x.Id == leaverequestId);
            if (lr == null)
            {
                return ReturnedResponse.ErrorResponse("This Leave Request Does not exist", null);
            }
            if (lr.Status == LeaveStatus.Approved.ToString() || lr.Status == LeaveStatus.Rejected.ToString())
            {
                return ReturnedResponse.ErrorResponse("This Leave Status has already been created", null);
            }

            _context.LeaveRequests.Remove(lr);
            await _context.SaveChangesAsync();

            return ReturnedResponse.SuccessResponse(null, "Leave Request has been deleted successfully");
        }

        public async Task<ApiResponse> GetAllRequest(int pageNumber = 1, int pageSize = 20)
        {
            var lr = await _context.LeaveRequests.ToListAsync();
            var filteredResult = lr.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            return ReturnedResponse.SuccessResponse("Successful", new { tList = filteredResult, totalCount = lr.Count() });
        }

        public async Task<ApiResponse> CreateUser(CreateUserModel model)
        {
            var isUserExist = await _context.Users.AnyAsync(x => x.Email.ToLower() == model.Email.ToLower());
            if (isUserExist)
            {
                return ReturnedResponse.ErrorResponse("user already exist", null);
            }

            var user = new Employee();
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;
            user.EmployeeId = model.Email;
            user.JobRole = model.JobRole;
            user.PhoneNumber = model.PhoneNumber;
            user.UserType = model.UserType;
            user.RemainingAnnualLeave = 21;
            user.RemainingSickLeave = 5;
            user.AnnualLeave = 21;
            user.SickLeave = 5;
            
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return ReturnedResponse.SuccessResponse(null, model);
        }

        public async Task<ApiResponse> GetAllUsers(int pageNumber=1, int pageSize=20)
        {
            var users = await _context.Users.ToListAsync();
            var empList = new List<EmployeeResponseModel>();
            foreach(var user in users)
            {
                EmployeeResponseModel emp = new EmployeeResponseModel();
                emp.AnnualLeave = user.AnnualLeave;
                emp.EmployeeId = user.EmployeeId;
                emp.FirstName = user.FirstName;
                emp.JobRole = user.JobRole;
                emp.LastName = user.LastName;
                emp.RemainingAnnualLeave = user.RemainingAnnualLeave;
                emp.RemainingSickLeave = user.RemainingSickLeave;
                emp.SickLeave = user.SickLeave;
                emp.TotalAnnualLeaveTaken = user.TotalAnnualLeaveTaken;
                emp.TotalSickLeaveTaken = user.TotalSickLeaveTaken;
                emp.UserType = user.UserType;

                empList.Add(emp);
            }

            var filteredResult = empList.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            return ReturnedResponse.SuccessResponse("Successful", new { tList = filteredResult, totalCount = empList.Count() });
        }

        public async Task<ApiResponse> ManagerLogin(string userEmail)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == userEmail && x.UserType == RoleType.Manager.ToString());
            if (user == null)
            {
                return ReturnedResponse.ErrorResponse("Inavlid login detail or user is not a Manager", null);
            }
            EmployeeResponseModel emp = new EmployeeResponseModel();
            emp.AnnualLeave = user.AnnualLeave;
            emp.EmployeeId = user.EmployeeId;
            emp.FirstName = user.FirstName;
            emp.JobRole = user.JobRole;
            emp.LastName = user.LastName;
            emp.RemainingAnnualLeave = user.RemainingAnnualLeave;
            emp.RemainingSickLeave = user.RemainingSickLeave;
            emp.SickLeave = user.SickLeave;
            emp.TotalAnnualLeaveTaken = user.TotalAnnualLeaveTaken;
            emp.TotalSickLeaveTaken = user.TotalSickLeaveTaken;
            emp.UserType = user.UserType;
            return ReturnedResponse.SuccessResponse(null, emp);

        }

    }
}
