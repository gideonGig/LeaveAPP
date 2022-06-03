using LeaveRequestAPP.Data;
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
    public class Utility
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<Utility> _log;
        public Utility(ApplicationDbContext context, ILogger<Utility> log)
        {
            _context = context;
            _log = log;
        }
           
        public  async Task<bool> CreateCalenderEvent(string userEmail, CalenderEvent eventObj)
        {
            try
            {
                var resp = await GetNewAccessToken(userEmail);
                if (resp.IsSuccessful)
                {                   
                    var theEventObj = JsonConvert.SerializeObject(eventObj);
                    var authString = $"Bearer {resp.AccessToken}";
                    var uri = $"https://graph.microsoft.com/v1.0/me/events";

                    var client = new RestClient(uri);
                    var request = new RestRequest();

                    request.AddHeader("Authorization", authString);
                    request.AddHeader("Content-type", "application/json");
                    request.AddJsonBody(eventObj, "application/json");

                    var clientResp = await client.PostAsync(request);

                    if (clientResp.StatusCode == System.Net.HttpStatusCode.Created)
                    {
                        return true;
                    }

                    _log.LogInformation($"Email: {userEmail} Error: {clientResp.Content} Date: {DateTime.Now.ToString()} EventBody: {eventObj}");
                    return false;

                }

                _log.LogInformation($"Email: {userEmail} Error: Invalid AccessToken Date: {DateTime.Now.ToString()} EventBody: {eventObj}");
                return false;
            }
            catch(Exception ex)
            {
                _log.LogInformation($"Email: {userEmail} Error: {ex.Message} Date: {DateTime.Now.ToString()} EventBody: {eventObj}");
                return false;
            }
           
        }

        public async Task<bool> SendMessageToOutLook(string userEmail, OutLookMessageRequestModel eventObj)
        {
            try
            {
                var resp = await GetNewAccessToken(userEmail);
                if (resp.IsSuccessful)
                {
                    var theEventObj = JsonConvert.SerializeObject(eventObj);
                    var authString = $"Bearer {resp.AccessToken}";
                    var uri = $"https://graph.microsoft.com/v1.0/me/sendMail";

                    var client = new RestClient(uri);
                    var request = new RestRequest();

                    request.AddHeader("Authorization", authString);
                    request.AddHeader("Content-type", "application/json");
                    request.AddJsonBody(eventObj, "application/json");

                    var clientResp = await client.PostAsync(request);

                    if (clientResp.StatusCode == System.Net.HttpStatusCode.Accepted)
                    {
                        return true;
                    }

                    _log.LogInformation($"Email: {userEmail} Error: {clientResp.Content} Date: {DateTime.Now.ToString()} EventBody: {eventObj}");
                    return false;
                }

                _log.LogInformation($"Email: {userEmail} Error: Invalid AccessToken Date: {DateTime.Now.ToString()} EventBody: {eventObj}");
                return false;
            }
            catch (Exception ex)
            {
                _log.LogInformation($"Email: {userEmail} Error: {ex.Message} Date: {DateTime.Now.ToString()} EventBody: {eventObj}");
                return false;
            }

        }

        public  async Task<GetAccessTokenResponseModel> GetNewAccessToken(string userEmail)
        {
            var b = Builder().Build().GetSection("AppSettings");
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == userEmail);
            if (user == null)
            {
                return new GetAccessTokenResponseModel { IsSuccessful = false, AccessToken = null };
            }

            if (!string.IsNullOrEmpty(user.AccessToken))
            {
                var client = new RestClient();

                var uri = $"https://login.microsoftonline.com/common/oauth2/v2.0/token";
                var request = new RestRequest(uri, Method.Post);
                request.AddParameter("client_id", b.GetSection("client_id").Value);
                request.AddParameter("scope", b.GetSection("scope").Value);
                request.AddParameter("redirect_uri", b.GetSection("redirect_uri").Value);
                request.AddParameter("refresh_token", user.RefreshToken);
                request.AddParameter("grant_type", "refresh_token");
                request.AddParameter("client_secret", b.GetSection("client_secret").Value);

                var resp = await client.ExecuteAsync(request);
                if (resp.IsSuccessful)
                {
                    var token = JsonConvert.DeserializeObject<AuthorizationTokenResponseModel>(resp.Content);

                    if (token != null)
                    {
                        user.AccessToken = token.access_token;
                        user.RefreshToken = token.refresh_token;
                        user.DateTokenReceived = DateTime.Now;
                        _context.Entry(user).State = EntityState.Modified;
                        await _context.SaveChangesAsync();

                        return new GetAccessTokenResponseModel { IsSuccessful = true, AccessToken = token.access_token };
                    }
                }

                return new GetAccessTokenResponseModel { IsSuccessful = false, AccessToken = null };
            }

            return new GetAccessTokenResponseModel { IsSuccessful = false, AccessToken = null };
        }

        public async Task<bool> SendEmail(EmailRequestModel msg)
        {
            try
            {
                using (var mail = new MailMessage())
                {
                    var builder = Builder();
                    string email = builder.Build().GetSection("AppSettings").GetSection("AppEmail").Value;
                    string password = builder.Build().GetSection("AppSettings").GetSection("AppPassword").Value;
                    string smptpServer = builder.Build().GetSection("AppSettings").GetSection("SmptpServer").Value;
                    int portNo = Int32.Parse(builder.Build().GetSection("AppSettings").GetSection("Port").Value);
                    //string pathToMail = mailPath;
                    var loginInfo = new NetworkCredential(email, password);
                    mail.From = new MailAddress(msg.FromAddress);
                    mail.To.Add(new MailAddress(msg.ManagerAddress));
                    mail.CC.Add(new MailAddress(msg.HrAddress));
                    mail.Subject = msg.Subject;
                    mail.IsBodyHtml = true;
                    mail.Body = msg.Body;
                    try
                    {
                        using (var smtpClient = new SmtpClient(smptpServer, portNo))
                        {
                            smtpClient.UseDefaultCredentials = false;
                            smtpClient.Credentials = loginInfo;
                            smtpClient.EnableSsl = true;
                            smtpClient.Send(mail);
                        };
                    }
                    finally
                    {
                        mail.Dispose();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                var exception = ex.Message;
                return false;
            }
        }

        public  DateTime Truncate(DateTime dateTime, TimeSpan timeSpan)
        {
            if (timeSpan == TimeSpan.Zero) return dateTime; // Or could throw an ArgumentException
            if (dateTime == DateTime.MinValue || dateTime == DateTime.MaxValue) return dateTime; // do not modify "guard" values
            return dateTime.AddTicks(-(dateTime.Ticks % timeSpan.Ticks));
        }
        public  IConfigurationBuilder Builder()
        {
            var builder = new ConfigurationBuilder()
                               .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                               .AddJsonFile("appsettings.json");

            return builder;
        }
    }
}
