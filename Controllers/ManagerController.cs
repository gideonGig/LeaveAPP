using LeaveRequestAPP.Interfaces;
using LeaveRequestAPP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LeaveRequestAPP.Controllers
{
    public class ManagerController : Controller
    {
        private readonly IManagerLeave _manager;
        private readonly ILogger<ManagerController> _log;
        public ManagerController(IManagerLeave manager, ILogger<ManagerController> log)
        {
            _manager = manager;
            _log = log;
        }

        /// <summary>
        /// This api approves a LeaveRequest, pass the leaverequest Id
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/v1/ApproveRequest")]
        public async Task<IActionResult> ApproveRequest([FromBody] RequestId model )
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errMessage = string.Join(" | ", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage));
                }

                var resp = await _manager.ApproveRequest(model.leaveRequestId, model.ManagerEmail);
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
                _log.LogInformation(string.Concat("Error Occured in ApproveRequest ", em));
                return BadRequest(ReturnedResponse.ErrorResponse("An error has occured", null));
            }
        }

        /// <summary>
        /// This api is used by the BackEnd to Approve request
        /// </summary>
        /// <param name="x_requestId"></param>
        /// <param name="x_managerEmail"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/v1/ApproveRequestBackEndOnly")]
        public async Task<IActionResult> ApproveRequestBackEndOnly(string x_requestId, string x_managerEmail)
        {
            try
            {
                return await ApproveRequest(new RequestId { leaveRequestId = x_requestId, ManagerEmail = x_managerEmail });
            }
            catch (Exception ex)
            {
                var em = ex.Message == null ? ex.InnerException.ToString() : ex.Message;
                _log.LogInformation(string.Concat("Error Occured in ApproveRequestBackEndOnly ", em));
                return BadRequest(ReturnedResponse.ErrorResponse("An error has occured", null));
            }
        }

        /// <summary>
        /// This api is used by the Backend to reject request
        /// </summary>
        /// <param name="x_requestId"></param>
        /// <param name="x_managerEmail"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/v1/RejectRequestBackEndOnly")]
        public async Task<IActionResult> RejectRequestBackEndOnly(string x_requestId, string x_managerEmail)
        {
            try
            {
                return await RejectRequest(new RequestId { leaveRequestId = x_requestId, ManagerEmail = x_managerEmail });
            }
            catch (Exception ex)
            {
                var em = ex.Message == null ? ex.InnerException.ToString() : ex.Message;
                _log.LogInformation(string.Concat("Error Occured in ApproveRequestBackEndOnly ", em));
                return BadRequest(ReturnedResponse.ErrorResponse("An error has occured", null));
            }
        }

        /// <summary>
        /// This api reject a leave request
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/v1/RejectRequest")]
        public async Task<IActionResult> RejectRequest([FromBody] RequestId model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errMessage = string.Join(" | ", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage));
                }

                var resp = await _manager.RejectRequest(model.leaveRequestId, model.ManagerEmail);
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
                _log.LogInformation(string.Concat("Error Occured in RejectRequest ", em));
                return BadRequest(ReturnedResponse.ErrorResponse("An error has occured", null));
            }
        }

        /// <summary>
        /// This api deletes a pending leave request from the system
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("api/v1/DeleteRequest")]
        public async Task<IActionResult> DeleteRequest([FromBody] RequestId model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errMessage = string.Join(" | ", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage));
                }

                var resp = await _manager.DeleteRequest(model.leaveRequestId);
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
                _log.LogInformation(string.Concat("Error Occured in DeleteRequest ", em));
                return BadRequest(ReturnedResponse.ErrorResponse("An error has occured", null));
            }
        }

        /// <summary>
        /// This APi gets all Leave request that has been created
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/v1/GetAllRequest")]
        public async Task<IActionResult> GetAllRequest(int pageNumber = 1, int pageSize = 40)
        {
            try
            {
                var resp = await _manager.GetAllRequest(pageNumber, pageSize);
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
                _log.LogInformation(string.Concat("Error Occured in GetAllRequest ", em));
                return BadRequest(ReturnedResponse.ErrorResponse("An error has occured", null));
            }
        }

        /// <summary>
        /// This api Creates a new User in the system
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <remarks>
        /// {
        ///   "FirstName" : "First Name of the user",
        ///   "LastName" : "Last Name of the user",
        ///   "JobRole" : "a drop down call api/v1/GetJobRole that shows the JobRole in the system/ you can type aa new JobRole also",
        ///   "Email" : "a valid accrete email address",
        ///   "PhoneNumber" : "phone number of user"
        ///   "UserType" : "a drop that showsif the use is a manager or user call api/v1/GetUserType
        /// }
        /// </remarks>
        [HttpPost]
        [Route("api/v1/CreateUser")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errMessage = string.Join(" | ", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage));
                }

                var resp = await _manager.CreateUser(model);
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
                _log.LogInformation(string.Concat("Error Occured in DeleteRequest ", em));
                return BadRequest(ReturnedResponse.ErrorResponse("An error has occured", null));
            }
        }

        /// <summary>
        /// This api get all users in the system
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/v1/GetAllUsers")]
        public async Task<IActionResult> GetAllUsers(int pageNumber = 1, int pageSize = 25)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errMessage = string.Join(" | ", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage));
                }

                var resp = await _manager.GetAllUsers(pageNumber, pageSize);
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
                _log.LogInformation(string.Concat("Error Occured in GetAllUsers ", em));
                return BadRequest(ReturnedResponse.ErrorResponse("An error has occured", null));
            }
        }


        [HttpGet]
        [Route("api/v1/GetAllPendingRequests")]
        public async Task<IActionResult> GetAllPendingRequests(int pageNumber =1, int pageSize = 15)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errMessage = string.Join(" | ", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage));
                }

                var resp = await _manager.GetAllPendingRequest(pageNumber, pageSize);
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
                _log.LogInformation(string.Concat("Error Occured in GetAllPendingRequests ", em));
                return BadRequest(ReturnedResponse.ErrorResponse("An error has occured", null));
            }
        }

    }
}
