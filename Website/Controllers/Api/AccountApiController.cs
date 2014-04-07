using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Newtonsoft.Json.Linq;
using Owin;
using Runnymede.Website.Models;
using Runnymede.Website.Utils;
using System;
using System.Data;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using System.Xml.Linq;

namespace Runnymede.Website.Controllers.Api
{
    [Authorize]
    [HostAuthentication(DefaultAuthenticationTypes.ApplicationCookie)]
    [RoutePrefix("api/AccountApi")]
    public class AccountApiController : ApiController
    {
        private const string LocalLoginProvider = "Local";

        public AccountApiController()
            //: this(Startup0.UserManagerFactory(), Startup0.OAuthOptions.AccessTokenFormat)
            : this(Startup.UserManagerFactory(), Startup.OAuthOptions.AccessTokenFormat)
        {
        }

        //public AccountApiController(UserManager<IdentityUser> userManager, ISecureDataFormat<AuthenticationTicket> accessTokenFormat)
        public AccountApiController(ApplicationUserManager userManager, ISecureDataFormat<AuthenticationTicket> accessTokenFormat)
        {
            UserManager = userManager;
            AccessTokenFormat = accessTokenFormat;
        }

        //public UserManager<IdentityUser> UserManager { get; private set; }
        public ApplicationUserManager UserManager { get; private set; }

        public ISecureDataFormat<AuthenticationTicket> AccessTokenFormat { get; private set; }

        // POST api/AccountApi/SignedIn
        [HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        [Route("SignedIn")]
        public IHttpActionResult SignedIn(SignedInBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var inferredTimeOffsetMin = LoggingUtils.InferTimeOffsetMin(model.LocalTime, model.LocalTimezoneOffset);
            var userId = this.GetUserId();

            string sql = @"
update dbo.appUsers set TimezoneOffsetMin = @TimezoneOffsetMin where Id = @UserId
";
            DapperHelper.ExecuteResiliently(sql, new
                {
                    TimezoneOffsetMin = inferredTimeOffsetMin,
                    UserId = userId
                });

            return Ok<object>(new { TimezoneOffsetMin = inferredTimeOffsetMin });
        }

        // POST api/AccountApi/
        [AllowAnonymous]
        public async Task<IHttpActionResult> PostCreate(CreateBindingModel model)
        {
            if (!ModelState.IsValid || !model.Consent)
            {
                return BadRequest(ModelState);
            }

            int userId = 0;
            var displayName = CoalesceDisplayName(model.DisplayName);

            // The internal Connection Resiliency strategy does not work with user-initiated transactions.
            // The workaround is described at "Limitations with Retrying Execution Strategies" +http://msdn.microsoft.com/en-us/data/dn307226.aspx
            AppDbConfiguration.SuspendExecutionStrategy = true; // Try/Finally is not needed. The value is stored per request.

            var executionStrategy = new SqlAzureExecutionStrategy();

            await executionStrategy.Execute(
                async () =>
                {
                    // The context should be constructed within the code block to be retried. This ensures that we are starting with a clean state for each retry.
                    // NOT using (var userManager = Request.GetOwinContext().GetUserManager<ApplicationUserManager>())
                    using (var userManager = new ApplicationUserManager())
                    {
                        // We use email as user name. By default AllowOnlyAlphanumericUserNames is true.
                        userManager.UserValidator = new UserValidator<ApplicationUser, int>(userManager) { AllowOnlyAlphanumericUserNames = false };
                        userManager.PasswordValidator = new MinimumLengthValidator(6);

                        var context = userManager.Context;
                        using (var transaction = context.Database.BeginTransaction())
                        {
                            // First phase. Identity user.
                            var identityUser = new ApplicationUser
                            {
                                UserName = model.UserName,
                                Email = model.UserName,
                            };

                            // Make DisplayName available in authorized requests without querying the database.
                            identityUser.Claims.Add(new CustomUserClaim { ClaimType = AppClaimTypes.DisplayName, ClaimValue = displayName });

                            IdentityResult result = await userManager.CreateAsync(identityUser, model.Password);

                            if (!result.Succeeded)
                            {
                                throw new Exception(result.Errors != null ? string.Join("\n", result.Errors) : "IdentityResult");
                            }

                            // Second phase. dbo.appUsers and accounting.
                            // identityUser.Id is auto-generated in dbo.aspnetUsers.

                            await context.Database.ExecuteSqlCommandAsync(
                                "execute dbo.appCreateUserAndAccounts @UserId = @p0, @DisplayName = @p1",
                                identityUser.Id, displayName);

                            transaction.Commit();

                            userId = identityUser.Id;
                        }
                    }
                });

            AppDbConfiguration.SuspendExecutionStrategy = false;

            // Logging.
            var logData =
                new XElement("LogData",
                    new XElement("Kind", LoggingUtils.Kind.Signup),
                    new XElement("Time", DateTime.UtcNow),
                    new XElement("Keeper", this.GetKeeper()),
                    new XElement("UserId", userId),
                    new XElement("TimeOffsetMin", LoggingUtils.InferTimeOffsetMin(model.LocalTime, model.LocalTimezoneOffset))
                    )
                    .ToString(SaveOptions.DisableFormatting);

            var logTask = LoggingUtils.WriteKeeperLogAsync(logData);

            // Send the confirmation link.
            var token = OwinUserManager.GetConfirmationToken<ApplicationUser, int>(userId);
            var queryString = IdentityHelper.GetMailLinkQueryString(token, userId);
            var host = Request.RequestUri.GetComponents(UriComponents.Host, UriFormat.Unescaped);
            var link = "https://" + host + "/account/confirm?" + queryString;
            var emailTask = this.SendConfirmationEmailAsync(model.UserName, link);

            // Parallel Async.
            await Task.WhenAll(logTask, emailTask);

            //return StatusCode(HttpStatusCode.NoContent); //JQuery 2.0 has changed behavior. Status OK 200 with empty response body is treated as a failure.
            //string redirectUrl = Url.Route("Default", new { controller = "Account", action = "Thanks" });
            //return Content<object>(HttpStatusCode.Created, new { redirectUrl = redirectUrl });
            return StatusCode(HttpStatusCode.Created);
        }

        // GET api/AccountApi/Profile
        [Route("Profile")]
        public async Task<IHttpActionResult> GetProfile()
        {
            var sql = @"
select @UserName as UserName, DisplayName, IsTeacher, Skype, TimezoneName, ReviewRate, SessionRate, Announcement
from dbo.appUsers
where Id = @Id;
";
            var result = (await DapperHelper.QueryResilientlyAsync<dynamic>(sql,
                new
                {
                    UserName = this.GetUserName(),
                    Id = this.GetUserId(),
                }))
                .Single();

            return Ok<object>(result);
        }


        // PUT api/AccountApi/Profile
        [Route("Profile")]
        public IHttpActionResult PutProfile([FromBody] JObject value)
        {
            // Update only changed fields. The null value indicates that the field has not been changed.
            var displayName = (string)value["displayName"];
            if (displayName != null)
            {
                displayName = CoalesceDisplayName(displayName);
            }

            var skype = (string)value["skype"];

            // If a parameter is null, that meens keep the corresponding field intact.
            DapperHelper.ExecuteResiliently("dbo.appUpdateUser", new
            {
                UserId = this.GetUserId(),
                DisplayName = displayName,
                Skype = skype,
            },
            CommandType.StoredProcedure);

            return StatusCode(HttpStatusCode.NoContent);
        }

        // PUT api/AccountApi/TeacherProfile
        [Route("TeacherProfile")]
        public IHttpActionResult PutTeacherProfile([FromBody] JObject value)
        {
            // Update only changed fields. The null value indicates the field has not been changed.
            var reviewRate = (decimal?)value["reviewRate"];
            var sessionRate = (decimal?)value["sessionRate"];
            var announcement = (string)value["announcement"];

            // If a parameter is null, that meens keep the corresponding field intact.
            DapperHelper.ExecuteResiliently("dbo.appUpdateUser", new
            {
                UserId = this.GetUserId(),
                ReviewRate = reviewRate,
                SessionRate = sessionRate,
                Announcement = announcement,
            },
            CommandType.StoredProcedure);

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST api/AccountApi/Password
        [Route("Password")]
        public async Task<IHttpActionResult> PostPassword(ChangePasswordBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var manager = OwinUserManager;
            var result = await manager.ChangePasswordAsync(this.GetUserId(), model.OldPassword, model.NewPassword1);

            if (result.Succeeded)
            {
                this.OwinAuthentication.SignOut();
            }

            return result.Succeeded ? StatusCode(HttpStatusCode.NoContent) : this.GetErrorResult(result);
        }

        // POST api/AccountApi/Forgot
        [AllowAnonymous]
        [Route("Forgot")]
        public async Task<IHttpActionResult> PostForgot(JObject value)
        {
            var email = (string)value["email"];
            if (!string.IsNullOrEmpty(email))
            {
                email = email.Trim();
                var userManager = this.OwinUserManager;
                ApplicationUser user = userManager.FindByName(email);
                if (user != null /*&& manager.IsConfirmed(user.Id)*/)
                {
                    // Send the link.
                    var token = userManager.GetPasswordResetToken<ApplicationUser, int>(user.Id);
                    var queryString = IdentityHelper.GetMailLinkQueryString(token, user.Id);
                    var host = Request.RequestUri.GetComponents(UriComponents.Host, UriFormat.Unescaped);
                    var link = "https://" + host + "/account/reset-password?" + queryString;
                    // TODO. Send email really asynchronousy.
                    await this.SendPasswordResetEmailAsync(email, link);
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST api/AccountApi/Reset
        [AllowAnonymous]
        [Route("Reset")]
        public async Task<IHttpActionResult> PostReset([FromUri] string code, [FromUri] string time, [FromBody] JObject value)
        {
            //string code = IdentityHelper.GetCodeFromRequest(Request);
            int userId = IdentityHelper.GetUserIdFromQueryStringValue(time);
            if (string.IsNullOrEmpty(code) || userId == 0)
            {
                return BadRequest();
            }

            var password1 = (string)value["password1"];
            var password2 = (string)value["password2"];
            if (password1 != password2)
            {
                return BadRequest("Passwords do not match.");
            }

            var result = await OwinUserManager.ResetPasswordAsync(userId, code, password1);
            return result.Succeeded ? StatusCode(HttpStatusCode.NoContent) : this.GetErrorResult(result);
        }

        // POST api/AccountApi/PaymentToTeacher
        [AllowAnonymous]
        [Route("PaymentToTeacher")]
        public async Task<IHttpActionResult> PostPaymentToTeacher(JObject value)
        {
            // Revalidate the sender's password.
            var password = ((string)value["password"]);
            var user = await UserManager.FindAsync(this.GetUserName(), password);
            if (user == null)
            {
                return BadRequest("Wrong password.");
            }

            // Parse the amount value entered by the user.
            var amountStr = ((string)value["amount"]).Trim().Replace(',', '.');
            decimal amount;
            if (!decimal.TryParse(amountStr, out amount))
                return BadRequest(amountStr);

            var teacherUserId = ((int)value["teacherUserId"]);

            DapperHelper.ExecuteResiliently("dbo.accMakePaymentToTeacher",
                new
                {
                    UserId = user.Id,
                    TeacherUserId = teacherUserId,
                    Amount = amount,
                },
                CommandType.StoredProcedure
                );

            return StatusCode(HttpStatusCode.NoContent);
        }

        // GET api/AccountApi/TransferRecepient
        [Route("TransferRecepient")]
        public async Task<IHttpActionResult> GetTransferRecepient()
        {
            var sql = @"
select @UserName as UserName, DisplayName, IsTeacher, Skype, TimezoneName, ReviewRate, Announcement
from dbo.appUsers
where Id = @Id;
";
            var result = (await DapperHelper.QueryResilientlyAsync<dynamic>(sql,
                new
                {
                    UserName = this.GetUserName(),
                    Id = this.GetUserId(),
                }))
                .Single();

            return Ok<object>(result);
        }




        private string CoalesceDisplayName(string postedDisplayName)
        {
            return string.IsNullOrWhiteSpace(postedDisplayName) ? "Anonymous" : postedDisplayName.Trim();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                UserManager.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Helpers

        private IAuthenticationManager OwinAuthentication
        {
            get { return Request.GetOwinContext().Authentication; }
        }

        private ApplicationUserManager OwinUserManager
        {
            get { return Request.GetOwinContext().GetUserManager<ApplicationUserManager>(); }
        }


        private IHttpActionResult GetErrorResult(IdentityResult result)
        {
            if (result == null)
            {
                return InternalServerError();
            }

            if (!result.Succeeded)
            {
                if (result.Errors != null)
                {
                    foreach (string error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }

                if (ModelState.IsValid)
                {
                    // No ModelState errors are available to send, so just return an empty BadRequest.
                    return BadRequest();
                }

                return BadRequest(ModelState);
            }

            return null;
        }

        #endregion
    }
}
