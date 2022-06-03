using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace LeaveRequestAPP.Models
{
    public enum LeaveStatus
    {
        Approved,
        Rejected
    }
    public enum LeaveType
    {
        AnnualLeave,
        SickLeave
    }

    public enum Status
    {
        Successful,
        Unsuccessful
    }

    public class UserEmailRequestModel
    {
        public string UserEmail { get; set; }
    }
    public class EmployeeResponseModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmployeeId { get; set; }
        public string JobRole { get; set; }
        public int RemainingSickLeave { get; set; }
        public int RemainingAnnualLeave { get; set; }
        public int AnnualLeave = 21;
        public int SickLeave = 5;
        public int TotalAnnualLeaveTaken { get; set; }
        public int TotalSickLeaveTaken { get; set; }
        public string UserType { get; set; }
        public string MicrosoftAuthString { get; set; }
    }
        

    public class RequestId
    {
        public string leaveRequestId { get; set; }
        public string ManagerEmail { get; set; }
    }
    public class CreateRequestModel
    {
        [Required]
        public DateTime? StartDate { get; set; }
        [Required]
        public DateTime? EndDate { get; set; }
        public string Comment { get; set; }
        [Required]
        public string LeaveType { get; set; }
        [Required]
        public string Manager { get; set; }
        [Required]
        public string HrAdmin { get; set; }
        [Required]
        public string EmployeeEmail { get; set; }
    }

    public class EditRequestModel
    {
        [Required]
        public string LeaveId { get; set; }
        [Required]
        public DateTime? StartDate { get; set; }
        [Required]
        public DateTime? EndDate { get; set; }
        public string Comment { get; set; }
        [Required]
        public string LeaveType { get; set; }
        [Required]
        public string Manager { get; set; }
        [Required]
        public string HrAdmin { get; set; }
        [Required]
        public string EmployeeEmail { get; set; }
    }

    public class CreateUserModel
    {
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string JobRole { get; set; }
        public string PhoneNumber { get; set; }
        [Required]
        public string UserType { get; set; }
    }

    public class ManagerResponseModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
    }

    public class EmailRequestModel
    {
        public string FromAddress { get; set; }
        public string ManagerAddress { get; set; }
        public string HrAddress { get; set; }
        public string Body { get; set; }
        public string Subject { get; set; }
    }

    public class AuthorizationTokenResponseModel
    {
        public string token_type { get; set; }
        public string scope { get; set; }
        public int expires_in { get; set; }
        public string access_token { get; set; }
        public string refresh_token { get; set; }
    }

    public class UserMicrosoftGraphResponseModel
    {
        [JsonProperty("@odata.context")]
        public string OdataContext { get; set; }
        public string id { get; set; }
        public List<string> businessPhones { get; set; }
        public string displayName { get; set; }
        public string givenName { get; set; }
        public string jobTitle { get; set; }
        public object mail { get; set; }
        public string mobilePhone { get; set; }
        public string officeLocation { get; set; }
        public object preferredLanguage { get; set; }
        public string surname { get; set; }
        public string userPrincipalName { get; set; }
    }

    public class Attendee
    {
        public EmailAddress emailAddress { get; set; }
        public string type { get; set; }
    }

    public class Body
    {
        public string contentType { get; set; }
        public string content { get; set; }
    }

    public class EmailAddress
    {
        public string address { get; set; }
        public string name { get; set; }
    }

    public class EventDate
    {
        public DateTime dateTime { get; set; }
        public string timeZone { get; set; }
    }

    public class Location
    {
        public string displayName { get; set; }
    }

    public class CalenderEvent
    {
        public string subject { get; set; }
        public Body body { get; set; }
        public EventDate start { get; set; }
        public EventDate end { get; set; }
        public Location location { get; set; }
        public List<Attendee> attendees { get; set; }
        public bool allowNewTimeProposals { get; set; }
    }

    public class GetAccessTokenResponseModel
    {
        public bool IsSuccessful { get; set; }
        public string AccessToken { get; set; }
    }

    //Create Email Body
    public class OutLookEmailAddress
    {
        public string address { get; set; }
    }

    public class Message
    {
        public string subject { get; set; }
        public Body body { get; set; }
        public List<ToPerson> toRecipients { get; set; }
        public List<ToPerson> ccRecipients { get; set; }
    }

    public class OutLookMessageRequestModel
    {
        public Message message { get; set; }
        public string saveToSentItems { get; set; }
    }

    public class ToPerson
    {
        public OutLookEmailAddress emailAddress { get; set; }
    }

}
