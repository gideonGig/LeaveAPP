using LeaveRequestAPP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LeaveRequestAPP.Interfaces
{
    public interface IUserLeave
    {
        Task<ApiResponse> CreateRequest(CreateRequestModel model);
        Task<ApiResponse> EditRequest(EditRequestModel model);    
        Task<ApiResponse> GetAllRequestByUserId(string userEmail, int pageNumber, int pageSize);
        Task<ApiResponse> GetAllAcceptedRequestByUserEmail(string userEmail, int pageNumber, int pageSize);
        Task<ApiResponse> GetAllRejectedRequestByUserEmail(string userEmail, int pageNumber, int pageSize);
        Task<ApiResponse> GetPendingLeavRequest(string userEmail, int pageNumber = 1, int pageSize = 40);
        Task<ApiResponse> GetColleaguesApprovedLeave(string userEmail, int pageNumber = 1, int pageSize = 40);
        Task<ApiResponse> GetHumanResource();
        Task<ApiResponse> GetUser(string userEmails);
        Task<ApiResponse> UserLogin(string userEmail);
        Task<ApiResponse> GetManagers();
        Task<ApiResponse> GetRole();
        Task CallBack(string code, string state, string error);
    }
    public interface IManagerLeave
    {
        Task<ApiResponse> ApproveRequest(string leaverequestId, string managerEmail);
        Task<ApiResponse> RejectRequest(string leaverequestId, string managerEmail);
        Task<ApiResponse> DeleteRequest(string leaverequestId);
        Task<ApiResponse> GetAllRequest(int pageNumber, int pageSize);
        Task<ApiResponse> CreateUser(CreateUserModel model);
        Task<ApiResponse> GetAllUsers(int pageNumber, int pageSize);
        Task<ApiResponse> ManagerLogin(string userEmail);
        Task<ApiResponse> GetAllPendingRequest(int pageNumber = 1, int pageSize = 40);
    }
}
