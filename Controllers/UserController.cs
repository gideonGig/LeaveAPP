using LeaveRequestAPP.Data;
using LeaveRequestAPP.Interfaces;
using LeaveRequestAPP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LeaveRequestAPP.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserLeave _user;
        private readonly ILogger<UserController> _log;
        public UserController(IUserLeave user, ILogger<UserController> log)
        {
            _user = user;
            _log = log;
        }

        /// <summary>
        /// This Api creates a new request for a user
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <remarks>
        /// "StartDate" : "Leave Start Date",
        /// "EndDate" : "Leave End Date",
        /// "Comment" : "Any Additiional Comment",
        /// "LeaveType" : "A drop down ( call api/v1/GetLeaveType to get LeaveType which can be AnnualLeave or SickLeave",
        /// "Manager" : "A drop down ( call api/v1/GetManager ), pass the Manger Email address, display the name",
        /// HrAdmin : "A drop down (call api/v1/GetHrAdmin ), Pass the HR email address, display the name",
        /// EmployeeEmail : "pass the employeeEmail, it is the unique identifier"
        /// </remarks>
        [HttpPost]
        [Route("api/v1/CreateRequest")]
        public async Task<IActionResult> CreateRequest([FromBody] CreateRequestModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errMessage = string.Join(" | ", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage));
                }

                var resp = await _user.CreateRequest(model);
                if (resp.Message == Status.Successful.ToString())
                {
                    return Ok(resp);
                }
                else
                {
                    return BadRequest(resp);
                }
            }
            catch (Exception ex)
            {
                var em = ex.Message == null ? ex.InnerException.ToString() : ex.Message;
                _log.LogInformation(string.Concat("Error Occured in CreateRequest ", em));
                return BadRequest(ReturnedResponse.ErrorResponse("An error has occured", null));
            }
        }

        /// <summary>
        /// Api Edits a Previously created Request that has not been approved or rejected
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <remarks>
        /// {
        ///    "LeaveId ": "pass the unique LeaveId of the Leave request",
        ///    "StartDate" : "LeaveStartDate",
        ///    "EndDate" : "LeaveEndDate",
        ///    "Comment" : "Any Additiional Comment",
        ///    "LeaveType" : "A drop down ( call api/v1/GetLeaveType to get LeaveType which can be AnnualLeave or SickLeave",
        ///    "Manager" : "A drop down ( call api/v1/GetManager ), pass the Manger Email address, display the name",
        ///    "HrAdmin" : "A drop down (call api/v1/GetHrAdmin ), Pass the HR email address, display the name",
        ///    "EmployeeEmail" : "pass the employeeEmail, it is the unique identifier"
        /// }
        /// </remarks>
        [HttpPut]
        [Route("api/v1/EditRequest")]
        public async Task<IActionResult> EditRequest([FromBody] EditRequestModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errMessage = string.Join(" | ", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage));
                }

                var resp = await _user.EditRequest(model);
                if (resp.Message == Status.Successful.ToString())
                {
                    return Ok(resp);
                }
                else
                {
                    return BadRequest(resp);
                }
            }
            catch (Exception ex)
            {
                var em = ex.Message == null ? ex.InnerException.ToString() : ex.Message;
                _log.LogInformation(string.Concat("Error Occured in EditRequest ", em));
                return BadRequest(ReturnedResponse.ErrorResponse("An error has occured", null));
            }
        }

        /// <summary>
        /// Api get the User's all LeaveRequest both Accepted, Rejected and NULL (pending)
        /// </summary>
        /// <param name="userEmail"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/v1/GetUserRequest")]
        public async Task<IActionResult> GetUserRequest(string userEmail, int pageNumber = 1, int pageSize = 40)
        {
            try
            {
              
                var resp = await _user.GetAllRequestByUserId(userEmail, pageNumber, pageSize);
                if (resp.Message == Status.Successful.ToString())
                {
                    return Ok(resp);
                }
                else
                {
                    return BadRequest(resp);
                }
            }
            catch (Exception ex)
            {
                var em = ex.Message == null ? ex.InnerException.ToString() : ex.Message;
                _log.LogInformation(string.Concat("Error Occured in GetUserRequest ", em));
                return BadRequest(ReturnedResponse.ErrorResponse("An error has occured", null));
            }
        }
        /// <summary>
        /// This api get only the accepted requested
        /// </summary>
        /// <param name="userEmail"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/v1/GetUserAcceptedRequest")]
        public async Task<IActionResult> GetUserAcceptedRequest(string userEmail, int pageNumber = 1, int pageSize = 40)
        {
            try
            {

                var resp = await _user.GetAllAcceptedRequestByUserEmail(userEmail, pageNumber, pageSize);
                if (resp.Message == Status.Successful.ToString())
                {
                    return Ok(resp);
                }
                else
                {
                    return BadRequest(resp);
                }
            }
            catch (Exception ex)
            {
                var em = ex.Message == null ? ex.InnerException.ToString() : ex.Message;
                _log.LogInformation(string.Concat("Error Occured in GetUserAcceptedRequest ", em));
                return BadRequest(ReturnedResponse.ErrorResponse("An error has occured", null));
            }
        }
        /// <summary>
        /// This api get only the rejected request
        /// </summary>
        /// <param name="userEmail"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/v1/GetUserRejectedRequest")]
        public async Task<IActionResult> GetUserRejectedRequest(string userEmail, int pageNumber = 1, int pageSize = 40)
        {
            try
            {

                var resp = await _user.GetAllRejectedRequestByUserEmail(userEmail, pageNumber, pageSize);
                if (resp.Message == Status.Successful.ToString())
                {
                    return Ok(resp);
                }
                else
                {
                    return BadRequest(resp);
                }
            }
            catch (Exception ex)
            {
                var em = ex.Message == null ? ex.InnerException.ToString() : ex.Message;
                _log.LogInformation(string.Concat("Error Occured in GetUserRejectedRequest ", em));
                return BadRequest(ReturnedResponse.ErrorResponse("An error has occured", null));
            }
        }

        /// <summary>
        /// This api get only the rejected request
        /// </summary>
        /// <param name="userEmail"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/v1/GetUserPendingRequest")]
        public async Task<IActionResult> GetUserPendingRequest(string userEmail, int pageNumber = 1, int pageSize = 40)
        {
            try
            {

                var resp = await _user.GetPendingLeavRequest(userEmail, pageNumber, pageSize);
                if (resp.Message == Status.Successful.ToString())
                {
                    return Ok(resp);
                }
                else
                {
                    return BadRequest(resp);
                }
            }
            catch (Exception ex)
            {
                var em = ex.Message == null ? ex.InnerException.ToString() : ex.Message;
                _log.LogInformation(string.Concat("Error Occured in GetUserPendingRequest ", em));
                return BadRequest(ReturnedResponse.ErrorResponse("An error has occured", null));
            }
        }

        /// <summary>
        /// This api get other colleagues approved Leave only
        /// </summary>
        /// <param name="userEmail"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/v1/GetColleaguesApprovedLeave")]
        public async Task<IActionResult> GetColleaguesApprovedLeave(string userEmail, int pageNumber = 1, int pageSize = 40)
        {
            try
            {

                var resp = await _user.GetColleaguesApprovedLeave(userEmail, pageNumber, pageSize);
                if (resp.Message == Status.Successful.ToString())
                {
                    return Ok(resp);
                }
                else
                {
                    return BadRequest(resp);
                }
            }
            catch (Exception ex)
            {
                var em = ex.Message == null ? ex.InnerException.ToString() : ex.Message;
                _log.LogInformation(string.Concat("Error Occured in GetUserPendingRequest ", em));
                return BadRequest(ReturnedResponse.ErrorResponse("An error has occured", null));
            }
        }



        /// <summary>
        /// This Api get the User Details
        /// </summary>
        /// <param name="userEmail"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/v1/GetUserByEmail")]
        public async Task<IActionResult> GetUserByEmail(string userEmail)
        {
            try
            {
                var resp = await _user.GetUser(userEmail);
                if (resp.Message == Status.Successful.ToString())
                {
                    return Ok(resp);
                }
                else
                {
                    return BadRequest(resp);
                }
            }
            catch (Exception ex)
            {
                var em = ex.Message == null ? ex.InnerException.ToString() : ex.Message;
                _log.LogInformation(string.Concat("Error Occured in GetUserByEmail ", em));
                return BadRequest(ReturnedResponse.ErrorResponse("An error has occured", null));
            }
        }

        /// <summary>
        /// This Api get the Leave Type that Exist in the system
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/v1/GetLeaveType")]
        public async Task<IActionResult> GetLeaveType()
        {
            try
            {
                var obj = new List<string> { LeaveType.AnnualLeave.ToString(), LeaveType.SickLeave.ToString() };
                return Ok(ReturnedResponse.SuccessResponse(null, obj));
            }
            catch (Exception ex)
            {
                var em = ex.Message == null ? ex.InnerException.ToString() : ex.Message;
                _log.LogInformation(string.Concat("Error Occured in GetLeaveType ", em));
                return BadRequest(ReturnedResponse.ErrorResponse("An error has occured", null));
            }
        }

        /// <summary>
        /// This Api get the user type that exist in the system
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/v1/GetUserType")]
        public async Task<IActionResult> GetUserType()
        {
            try
            {
                var obj = new List<string> { RoleType.Manager.ToString(), RoleType.User.ToString() };
                return Ok(ReturnedResponse.SuccessResponse(null,obj));
            }
            catch (Exception ex)
            {
                var em = ex.Message == null ? ex.InnerException.ToString() : ex.Message;
                _log.LogInformation(string.Concat("Error Occured in GetUserType ", em));
                return BadRequest(ReturnedResponse.ErrorResponse("An error has occured", null));
            }
        }



        /// <summary>
        /// This api checks a userEmail exist in the system and returns back the user detail, render the front end based on the userType if it is Manager or User
        /// </summary>
        /// <param name="userEmail"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/v1/UserLogin")]
        public async Task<IActionResult> UserLogin([FromBody]UserEmailRequestModel model)
        {
            try
            {
                var resp = await _user.UserLogin(model.UserEmail);
                if (resp.Message == Status.Successful.ToString())
                {
                    return Ok(resp);
                }
                else
                {
                    return BadRequest(resp);
                }

            }
            catch (Exception ex)
            {
                var em = ex.Message == null ? ex.InnerException.ToString() : ex.Message;
                _log.LogInformation(string.Concat("Error Occured in UserLogin ", em));
                return BadRequest(ReturnedResponse.ErrorResponse("An error has occured", null));
            }
        }

        /// <summary>
        /// This aoi gets all Managers in the system
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/v1/GetManagers")]
        public async Task<IActionResult> GetManagers()
        {
            try
            {
                var resp = await _user.GetManagers();
                if (resp.Message == Status.Successful.ToString())
                {
                    return Ok(resp);
                }
                else
                {
                    return BadRequest(resp);
                }

            }
            catch (Exception ex)
            {
                var em = ex.Message == null ? ex.InnerException.ToString() : ex.Message;
                _log.LogInformation(string.Concat("Error Occured in GetManagers ", em));
                return BadRequest(ReturnedResponse.ErrorResponse("An error has occured", null));
            }
        }

        /// <summary>
        /// This api get all Roles in the system
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/v1/GetRoles")]
        public async Task<IActionResult> GetRoles()
        {
            try
            {
                var resp = await _user.GetRole();
                if (resp.Message == Status.Successful.ToString())
                {
                    return Ok(resp);
                }
                else
                {
                    return BadRequest(resp);
                }

            }
            catch (Exception ex)
            {
                var em = ex.Message == null ? ex.InnerException.ToString() : ex.Message;
                _log.LogInformation(string.Concat("Error Occured in GetManagers ", em));
                return BadRequest(ReturnedResponse.ErrorResponse("An error has occured", null));
            }
        }

        /// <summary>
        /// The api is used to get all Hr Personnel in the system
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/v1/GetHrAdmin")]
        public async Task<IActionResult> GetHrAdmin()
        {
            try
            {

                var resp = await _user.GetHumanResource();
                if (resp.Message == Status.Successful.ToString())
                {
                    return Ok(resp);
                }
                else
                {
                    return BadRequest(resp);
                }
            }
            catch (Exception ex)
            {
                var em = ex.Message == null ? ex.InnerException.ToString() : ex.Message;
                _log.LogInformation(string.Concat("Error Occured in GetHrAdmin ", em));
                return BadRequest(ReturnedResponse.ErrorResponse("An error has occured", null));
            }
        }



        [HttpGet]
        [Route("api/v1/CallBack")]
        public async Task<IActionResult> CallBack(string code, string state, string error)
        {
            try
            {
                await _user.CallBack(code, state, error);
                return Ok($"{code} {state} {error}");

            }
            catch (Exception ex)
            {
                var em = ex.Message == null ? ex.InnerException.ToString() : ex.Message;
                _log.LogInformation(string.Concat("Error Occured in GetManagers ", em));
                return BadRequest(ReturnedResponse.ErrorResponse("An error has occured", null));
            }
        }


    }
}
