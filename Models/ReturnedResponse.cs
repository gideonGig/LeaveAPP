using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LeaveRequestAPP.Models
{
    public static class ReturnedResponse
    {
        public static ApiResponse ErrorResponse(string message, object data)
        {
            var apiResp = new ApiResponse();
            apiResp.data = data;
            apiResp.Message = Status.Unsuccessful.ToString();
            apiResp.code = "400";
            var error = new ApiError();
            error.message = message;
            apiResp.error = error;

            return apiResp;
        }

        public static ApiResponse SuccessResponse(string message, object data, string ReferenceId = "")
        {
            var apiResp = new ApiResponse();
            apiResp.data = data;
            apiResp.referenceId = ReferenceId;
            apiResp.Message = Status.Successful.ToString();
            apiResp.code = "200";
            var error = new ApiError();
            error.message = message;
            apiResp.error = error;

            return apiResp;
        }
    }
    public class ApiResponse
    {
        public ApiResponse()
        {
            ApiVersion = "v1";
        }
        public string ApiVersion { get; set; }
        public string referenceId { get; set; }
        public string code { get; set; }
        public string Message { get; set; }
        public object data { get; set; }
        public ApiError error { get; set; }
    }
    public class ApiError
    {
        public string message { get; set; }
    }
}
