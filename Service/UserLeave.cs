using LeaveRequestAPP.Data;
using LeaveRequestAPP.Interfaces;
using LeaveRequestAPP.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace LeaveRequestAPP.Service
{
    public class UserLeave : IUserLeave
    {
        private  ApplicationDbContext _context;
        private readonly ILogger<UserLeave> _log;
        private readonly Utility _util;
        public UserLeave(ApplicationDbContext context, ILogger<UserLeave> log, Utility util)
        {
            _context = context;
            _log = log;
            _util = util;
        }
        public async Task<ApiResponse> CreateRequest(CreateRequestModel model)
        {
            var b = Builder().Build().GetSection("AppSettings");
            if (model.StartDate > model.EndDate)
            {
                ReturnedResponse.ErrorResponse("Start date cannot be greater than Enddate", null);
            }

           
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == model.EmployeeEmail);
            if (user == null)
            {
                return ReturnedResponse.ErrorResponse("This user does not exist", null);
            }

            //Check if a Leave for this user is already in Progress, i.e start date or end date is in between previous leave
            var lastRequest = await _context.LeaveRequests.OrderByDescending(x => x.StartDate).FirstOrDefaultAsync(x => x.EmployeeId == model.EmployeeEmail);
            if (lastRequest != null)
            {
                if ((model.StartDate >= lastRequest.StartDate && model.StartDate <= lastRequest.EndDate) || (model.EndDate >= lastRequest.StartDate && model.EndDate <= lastRequest.EndDate))
                {
                    return ReturnedResponse.ErrorResponse("An existing leave request whose date range falls within start date or end date already exist", null);
                }
               
            }

            string sickLeaveComment = "";
            bool isSickLeaveExhausted = false;
            var lr = new LeaveRequest();
            int weekendDays = 0;
            for (DateTime? date = model.StartDate; date.Value.Date <= model.EndDate; date = date.Value.AddDays(1))
            {
                if ((date.Value.DayOfWeek == DayOfWeek.Saturday) || (date.Value.DayOfWeek == DayOfWeek.Sunday))
                {

                }
                else
                {
                    weekendDays++;
                }
            }
            if (weekendDays > 10)
            {
                return ReturnedResponse.ErrorResponse("Leave days should not exceed 10 days", null);
            }
            if (model.LeaveType == LeaveType.AnnualLeave.ToString())
            {
                //check if Leave Taken is more than your remaining leave
                if (weekendDays > user.AnnualLeave - user.TotalAnnualLeaveTaken)
                {
                    return ReturnedResponse.ErrorResponse("Total leave days is greater than your Remaining Leave", null);
                }

                //check if 21 days Leave is complete
                if (user.TotalAnnualLeaveTaken >= 21)
                {
                    return ReturnedResponse.ErrorResponse("Your Annual Leave has been Exhausted, Please contact you HR Admin", null);
                }            
            }

            //Check if the Sick Leave has been Exhauseted, inform the User that days will be deducted from Sick Leave
            if (model.LeaveType == LeaveType.SickLeave.ToString())
            {
                if (user.TotalSickLeaveTaken >= 5)
                {
                    sickLeaveComment = "Sick Leave has been Exhausted, days wil be deducted from Annual Leave";
                    isSickLeaveExhausted = true;

                    //Now we know the TotalSickLeaveTaken has been exhausted, Check if the days taken is more then the TotalAnnualLeaveTaken
                    if (weekendDays > user.AnnualLeave - user.TotalAnnualLeaveTaken || user.TotalAnnualLeaveTaken >= 21)
                    {
                        sickLeaveComment = "Sick Leave has been Exhausted, days cannot be deducted from Annual Leave, " +
                            "Annual Leave period has been exhausted or Total days taken is greater than Annual Leave Days";
                        isSickLeaveExhausted = true;
                    }

                }
            }
 
            lr.NoOfDays = weekendDays;
            lr.Id = Guid.NewGuid().ToString();
            lr.HrAdmin = model.HrAdmin;
            lr.LeaveType = model.LeaveType;
            lr.StartDate = model.StartDate;
            lr.EndDate = model.EndDate;
            lr.EmployeeId = model.EmployeeEmail;
            lr.Comment = model.Comment;
            lr.ApprovedBy = model.Manager;
            lr.DateCreated = DateTime.Now;

            await _context.LeaveRequests.AddAsync(lr);
            await _context.SaveChangesAsync();

            //Send Email To OutLook
            var ls = model.LeaveType == LeaveType.AnnualLeave.ToString() ? "an" : "a";
            OutLookMessageRequestModel mail = new OutLookMessageRequestModel();
            Message oMessage = new Message();
            oMessage.body = new Body
            {
                content = $"{user.FirstName} {user.LastName} is requesting {ls} {model.LeaveType} leave between {model.StartDate.Value} to {model.EndDate} for {lr.NoOfDays} day(s) </br>" +
                $" Comments : {model.Comment}</br> <form action=\"{b.GetSection("backendurl").Value}ApproveRequestBackEndOnly\" method = \"GET\"> " +
                $"<input type=\"hidden\" name=\"requestId\" value=\"{lr.Id}\" /> <input type=\"hidden\" name=\"managerEmail\" value=\"{model.Manager}\" />"  + 
                $"<input type=\"submit\" value=\"Approve\" /></form>" + "  " + 
                $"<form action=\"{b.GetSection("backendurl").Value}RejectRequestBackEndOnly\" method = \"GET\"> " +
                $"<input type=\"hidden\" name=\"requestId\" value=\"{lr.Id}\" /> <input type=\"hidden\" name=\"managerEmail\" value=\"{model.Manager}\" />" + 
                $"<input type=\"submit\" value=\"Reject\" /></form>",
                contentType = "HTML"
            };
            oMessage.subject = $"{user.FirstName} {model.LeaveType} Leave";

            ToPerson manager = new ToPerson { emailAddress = new OutLookEmailAddress { address = model.Manager} };
            ToPerson hrAdmin = new ToPerson { emailAddress = new OutLookEmailAddress { address = model.HrAdmin } };
            var recipientList = new List<ToPerson>();
            recipientList.Add(manager);
            var ccList = new List<ToPerson>();
            ccList.Add(hrAdmin);
            oMessage.toRecipients = recipientList;
            oMessage.ccRecipients = ccList;

            mail.message = oMessage;
            mail.saveToSentItems = "false";

            await _util.SendMessageToOutLook(model.EmployeeEmail, mail);

            return ReturnedResponse.SuccessResponse(null, "Leave Request Has Been Sent to Manager");
        }

        public async Task<ApiResponse> EditRequest(EditRequestModel model)
        {
            if (model.StartDate.Value.Date > model.EndDate.Value.Date)
            {
                ReturnedResponse.ErrorResponse("Start date cannot be greater than Enddate", null);
            }
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == model.EmployeeEmail);
            if (user == null)
            {
                return ReturnedResponse.ErrorResponse("This user does not exist", null);
            }
            string sickLeaveComment = "";
            bool isSickLeaveExhausted = false;
            var lr = await _context.LeaveRequests.FirstOrDefaultAsync(x => x.Id == model.LeaveId);
            if (lr == null)
            {
                return ReturnedResponse.ErrorResponse("This Leave Request Does not exist", null);
            }
            if (lr.Status == LeaveStatus.Approved.ToString())
            {
                return ReturnedResponse.ErrorResponse("Leave has already been Approved cannot be Edited", null);
            }
            
            lr.StartDate = model.StartDate;
            lr.EndDate = model.EndDate;
            int days = 0;
            for (DateTime? date = model.StartDate; date.Value.Date <= model.EndDate.Value.Date; date = date.Value.AddDays(1))
            {
                if ((date.Value.DayOfWeek == DayOfWeek.Saturday) || (date.Value.DayOfWeek == DayOfWeek.Sunday))
                {
                    days++;
                }
            }
            if (days > 10)
            {
                return ReturnedResponse.ErrorResponse("Leave should not exceed 10 days", null);
            }
           
            if (model.LeaveType == LeaveType.AnnualLeave.ToString())
            {
                if (days > user.AnnualLeave - user.TotalAnnualLeaveTaken)
                {
                    return ReturnedResponse.ErrorResponse("Total leave days is greater than your Remaining Leave", null);
                }

                if (user.TotalAnnualLeaveTaken >= 21)
                {
                    return ReturnedResponse.ErrorResponse("Your Annual Leave has been Exhausted, Please contact you HR Admin", null);
                }
            }

            if (model.LeaveType == LeaveType.SickLeave.ToString())
            {
                if (user.TotalAnnualLeaveTaken >= 21)
                {
                    sickLeaveComment = "Sick Leave has been Exhausted, days wil be deducted from Annual Leave";
                    isSickLeaveExhausted = true;
                }
            }
            lr.NoOfDays = days;
            lr.HrAdmin = model.HrAdmin;
            lr.LeaveType = model.LeaveType;
            lr.StartDate = model.StartDate;
            lr.EndDate = model.EndDate;
            lr.EmployeeId = model.EmployeeEmail;
            lr.Comment = model.Comment;
            lr.ApprovedBy = model.Manager;
            lr.Status = null;
            lr.DateCreated = DateTime.Now;

            _context.Entry(lr).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            if (isSickLeaveExhausted)
            {
                return ReturnedResponse.SuccessResponse(null, sickLeaveComment);
            }
            return ReturnedResponse.SuccessResponse(null, "Leave has been Updated Successfully");

        }

        public async Task<ApiResponse> GetAllAcceptedRequestByUserEmail(string userEmail, int pageNumber=1, int pageSize=20)
        {
            var listLR = await _context.LeaveRequests.Where(x => x.EmployeeId == userEmail && x.Status == LeaveStatus.Approved.ToString()).ToListAsync();
            var filteredResult = listLR.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            return ReturnedResponse.SuccessResponse("Successful", new { tList = filteredResult, totalCount = listLR.Count() });
        }

        public async Task<ApiResponse> GetAllRejectedRequestByUserEmail(string userEmail, int pageNumber = 1, int pageSize = 20)
        {
            var listLR = await _context.LeaveRequests.Where(x => x.EmployeeId == userEmail && x.Status == LeaveStatus.Rejected.ToString()).ToListAsync();
            var filteredResult = listLR.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            return ReturnedResponse.SuccessResponse("Successful", new { tList = filteredResult, totalCount = listLR.Count() });
        }

        public async Task<ApiResponse> GetAllRequestByUserId(string userEmail, int pageNumber = 1, int pageSize = 20)
        {
            var listLR = await _context.LeaveRequests.Where(x => x.EmployeeId == userEmail).ToListAsync();
            var filteredResult = listLR.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            return ReturnedResponse.SuccessResponse("Successful", new { tList = filteredResult, totalCount = listLR.Count() });
        }

        public async Task<ApiResponse> GetPendingLeavRequest(string userEmail, int pageNumber = 1, int pageSize = 20)
        {
            var listLR = await _context.LeaveRequests.Where(x => x.EmployeeId == userEmail && x.Status == null).ToListAsync();
            var filteredResult = listLR.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            return ReturnedResponse.SuccessResponse("Successful", new { tList = filteredResult, totalCount = listLR.Count() });
        }

        public async Task<ApiResponse> GetColleaguesApprovedLeave(string userEmail, int pageNumber = 1, int pageSize = 20)
        {
            var userExist = await _context.Users.AnyAsync(x => x.Email == userEmail);
            if (userExist)
            {
                var listLR = await _context.LeaveRequests.Where(x => x.EmployeeId != userEmail && x.Status == LeaveStatus.Approved.ToString()).ToListAsync();
                var filteredResult = listLR.Skip((pageNumber - 1) * pageSize).Take(pageSize);
                return ReturnedResponse.SuccessResponse("Successful", new { tList = filteredResult, totalCount = listLR.Count() });
            }

            return ReturnedResponse.ErrorResponse("This user does not exist", null);
        }

        public async Task<ApiResponse> GetUser(string userEmail)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == userEmail);
          
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

        public async Task<ApiResponse> UserLogin(string userEmail)
        {
            var b = Builder().Build().GetSection("AppSettings");
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == userEmail);
            if (user == null)
            {
                return ReturnedResponse.ErrorResponse("Inavlid login detail", null);
            }
            var authString = $"https://login.microsoftonline.com/common/oauth2/v2.0/authorize?client_id={b.GetSection("client_id").Value}&response_type=code" +
                $"&redirect_uri={b.GetSection("redirect_uri").Value}&response_mode=query&scope={b.GetSection("scope").Value}&state={userEmail}";

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
            emp.MicrosoftAuthString = authString;
            return ReturnedResponse.SuccessResponse(null, emp);
        }

        public async Task<ApiResponse> GetManagers()
        {
            var users = await _context.Users.Where(x => x.UserType == RoleType.Manager.ToString()).ToListAsync();
            if (users == null || users.Count == 0)
            {
                return ReturnedResponse.ErrorResponse(null, "No users with User Type Manager in the system");
            }
            var manager = new List<ManagerResponseModel>();     
            foreach(var user in users)
            {
                var m = new ManagerResponseModel();
                m.Email = user.Email;
                m.FirstName = user.FirstName;
                m.LastName = user.LastName;
                manager.Add(m);
            }
            return ReturnedResponse.SuccessResponse(null, manager);
        }

        public async Task<ApiResponse> GetHumanResource()
        {
            var users = await _context.Users.Where(x => x.JobRole == "Human Resource").ToListAsync();
            if (users == null || users.Count == 0)
            {
                return ReturnedResponse.ErrorResponse(null, "No users with User Type Manager in the system");
            }
            var manager = new List<ManagerResponseModel>();
            foreach (var user in users)
            {
                var m = new ManagerResponseModel();
                m.Email = user.Email;
                m.FirstName = user.FirstName;
                m.LastName = user.LastName;
                manager.Add(m);
            }
            return ReturnedResponse.SuccessResponse(null, manager);
        }
        public async Task<ApiResponse> GetRole()
        {
            var roles = await _context.Roles.Where(x => x.Name != RoleType.Manager.ToString() && x.Name != RoleType.User.ToString()).ToListAsync();
            var roleNames = new List<string>();
            foreach(var role in roles)
            {
                roleNames.Add(role.Name);
            }

            return ReturnedResponse.SuccessResponse(null, roleNames);
        }

        public async Task CallBack(string code, string state, string error)
        {
            var b = Builder().Build().GetSection("AppSettings");
           
            if (!string.IsNullOrEmpty(code))
            {
                var client = new RestClient();

                var uri = $"https://login.microsoftonline.com/common/oauth2/v2.0/token";
                var request = new RestRequest(uri, Method.Post);
                request.AddParameter("client_id", b.GetSection("client_id").Value);
                request.AddParameter("scope", b.GetSection("scope").Value);
                request.AddParameter("redirect_uri", b.GetSection("redirect_uri").Value);
                request.AddParameter("code", code);
                request.AddParameter("grant_type", "authorization_code");
                request.AddParameter("client_secret", b.GetSection("client_secret").Value);

                var resp = await client.ExecuteAsync(request);
                if (resp.IsSuccessful)
                {
                     await SaveToken(state, resp.Content);
                }             
            }
        }

        public async Task SaveToken(string email, string response)
        {
            try 
            {
                var userToken = _context.Users.FirstOrDefault(x => x.Email == email);
                var token = JsonConvert.DeserializeObject<AuthorizationTokenResponseModel>(response);
            
                if (userToken != null)
                {
                    userToken.AccessToken = token.access_token;
                    userToken.RefreshToken = token.refresh_token;
                    userToken.DateTokenReceived = DateTime.Now;
                    _context.Entry(userToken).State = EntityState.Modified;
                    await _context.SaveChangesAsync();

                    await GetUserByViaMicrosoftGraph(email);
                }
            }

            catch (Exception ex)
            {
                _log.LogInformation(ex.Message);
            }
        }

        public async Task<ApiResponse> GetUserByViaMicrosoftGraph(string userEmail)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == userEmail);
            if (user == null)
            {
                return ReturnedResponse.ErrorResponse("user does not exist", null);
            }

            var client = new RestClient();

            var uri = $"https://graph.microsoft.com/v1.0/me";
            var request = new RestRequest(uri, Method.Get);
            request.AddHeader("Authorization", $"Bearer {user.AccessToken}");
            var resp = await client.ExecuteAsync(request);
            if (resp.IsSuccessful)
            {
                var graphUser = JsonConvert.DeserializeObject<UserMicrosoftGraphResponseModel>(resp.Content);
                return ReturnedResponse.SuccessResponse(null, graphUser);
            }

            return ReturnedResponse.ErrorResponse(null, resp.Content);
        }


 
        
        public IConfigurationBuilder Builder()
        {
            var builder = new ConfigurationBuilder()
                               .SetBasePath(Directory.GetCurrentDirectory())
                               .AddJsonFile("appsettings.json");

            return builder;
        }

    }
}
