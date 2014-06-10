using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Newtonsoft.Json.Linq;
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

        private ApplicationUserManager owinUserManager;
        private ApplicationUserManager OwinUserManager
        {
            get
            {
                return owinUserManager ?? (owinUserManager = Request.GetOwinContext().GetUserManager<ApplicationUserManager>());
            }
        }

        private IAuthenticationManager owinAuthenticationManager;
        private IAuthenticationManager OwinAuthenticationManager
        {
            get
            {
                return owinAuthenticationManager ?? (owinAuthenticationManager = Request.GetOwinContext().Authentication);
            }
        }

        // POST api/AccountApi/Signup
        [Route("Signup")]
        [AllowAnonymous]
        public async Task<IHttpActionResult> PostSignup(SignupBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var loginHelper = new LoginHelper(OwinUserManager, OwinAuthenticationManager);

            // Create the user account
            var user = await loginHelper.Signup(model, null, this.GetKeeper(), Request.RequestUri);

            var error = loginHelper.InspectErrorAfterSignup(user);
            if (!string.IsNullOrEmpty(error))
            {
                throw new Exception(error);
            }

            return StatusCode(HttpStatusCode.Created);
        }

        // POST api/AccountApi/Login
        [AllowAnonymous]
        [Route("Login")]
        public async Task<IHttpActionResult> PostLogin(JObject value)
        {
            var userName = (string)value["userName"];
            var password = (string)value["password"];
            var persistent = (bool)value["persistent"];

            ApplicationUser user = await OwinUserManager.FindAsync(userName, password);
            if (user == null)
            {
                return BadRequest("The email address or password is incorrect.");
            }

            await new LoginHelper(OwinUserManager, OwinAuthenticationManager).Sigin(user, persistent);

            return StatusCode(HttpStatusCode.NoContent);

            /*
            Just in case. In the opposite case of token/bearer authentication. How to pass custom values to the OWIN middleware.             
            data: {
                grant_type: 'password',
                userName: userName,
                password: password,
                scope: persistent ? 'persistent_cookie' : 'session_cookie', // Pass our custom value. Scope may be a list of values separated by spaces.
            },               
            public override void GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
            {
                // AF. A hack to enable the option of session or persistent cookies. We piggy-back the request and pass our custom value. Scope may be sent as a list of values separated by spaces.
                var isPersistent = context.Scope.Any(i => i == "persistent_cookie");             
            }               
            */


        }

        // GET api/AccountApi/PersonalProfile
        [Route("PersonalProfile")]
        public async Task<IHttpActionResult> GetPersonalProfile()
        {
            var sql = @"
select U.DisplayName, U.IsTeacher, U.Skype, U.ExtIdUpperCase, AU.Email, AU.EmailConfirmed
from dbo.appUsers U
	inner join dbo.aspnetUsers AU on U.Id = AU.Id
where U.Id = @Id;
";
            var result = (await DapperHelper.QueryResilientlyAsync<dynamic>(sql,
                new
                {
                    Id = this.GetUserId(),
                }))
                .Select(i => new
                {
                    UserName = this.GetUserName(),
                    DisplayName = (string)i.DisplayName,
                    IsTeacher = (bool)i.IsTeacher,
                    Skype = (string)i.Skype,
                    Email = (string)i.Email,
                    EmailConfirmed = (bool)i.EmailConfirmed,
                    AvatarLargeUrl = AzureStorageUtils.GetContainerBaseUrl(AzureStorageUtils.ContainerNames.AvatarsLarge, true) + i.ExtIdUpperCase,
                    AvatarSmallUrl = AzureStorageUtils.GetContainerBaseUrl(AzureStorageUtils.ContainerNames.AvatarsSmall, true) + i.ExtIdUpperCase,
                })
                .Single();

            return Ok<object>(result);
        }

        // GET api/AccountApi/TeacherProfile
        [Route("TeacherProfile")]
        public async Task<IHttpActionResult> GetTeacherProfile()
        {
            var sql = @"
select U.IsTeacher, U.ReviewRate, U.SessionRate, U.Announcement, PhoneNumber, PhoneNumberConfirmed
from dbo.appUsers U
	inner join dbo.aspnetUsers AU on U.Id = AU.Id
where U.Id = @Id;
";
            var result = (await DapperHelper.QueryResilientlyAsync<dynamic>(sql, new { Id = this.GetUserId(), }))
                .Single();

            return Ok<object>(result);
        }

        // GET api/AccountApi/Logins
        [Route("Logins")]
        public async Task<IHttpActionResult> GetLogins()
        {
            var userId = this.GetUserId();

            var userLogins = (await OwinUserManager.GetLoginsAsync(userId))
                .Select(i => new
                {
                    LoginProvider = i.LoginProvider,
                    ProviderKey = i.ProviderKey,
                })
                .ToList();

            var otherLogins = OwinAuthenticationManager.GetExternalAuthenticationTypes()
                .Where(i => userLogins.All(u => u.LoginProvider != i.AuthenticationType))
                .Select(i => i.AuthenticationType)
                .ToList();


            var hasPassword = await OwinUserManager.HasPasswordAsync(userId);
            //var user = OwinUserManager.FindById(this.GetUserId());
            //var hasPassword = (user != null) && !string.IsNullOrEmpty(user.PasswordHash);

            var result = new
            {
                UserLogins = userLogins,
                OtherLogins = otherLogins,
                HasPassword = hasPassword,
            };

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
                displayName = LoginHelper.CoalesceDisplayName(displayName);
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

        // PUT api/AccountApi/Password
        [Route("Password")]
        public async Task<IHttpActionResult> PutPassword(JObject value)
        {
            var oldPassword = (string)value["oldPassword"];
            var newPassword = (string)value["newPassword"];
            var userId = this.GetUserId();

            var result = string.IsNullOrEmpty(oldPassword)
                ? await OwinUserManager.AddPasswordAsync(userId, newPassword)
                : await OwinUserManager.ChangePasswordAsync(userId, oldPassword, newPassword);

            if (result.Succeeded)
            {
                OwinAuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                return StatusCode(HttpStatusCode.NoContent);
            }
            else
            {
                return BadRequest(result.PlainErrorMessage());
            }
        }

        // POST api/AccountApi/PhoneNumber
        [Route("PhoneNumber")]
        public async Task<IHttpActionResult> PostPhoneNumber(JObject value)
        {
            var phoneNumber = (string)value["phoneNumber"];
            string code = null;
            if (!string.IsNullOrEmpty(phoneNumber))
            {
                code = await OwinUserManager.GenerateChangePhoneNumberTokenAsync(this.GetUserId(), phoneNumber.Trim());
            }
            if (!string.IsNullOrEmpty(code))
            {
                return StatusCode(HttpStatusCode.NoContent);
                // Do not return the code to the client while in production! Send it via SMS.
                //return Ok<string>(code); 
            }
            else
                return BadRequest("Verification code not sent.");

            // A phone number can be removed by calling UserManager.SetPhoneNumberAsync(userId, null);
        }

        // POST api/AccountApi/PhoneVerification
        [Route("PhoneVerification")]
        public async Task<IHttpActionResult> PostPhoneVerification(JObject value)
        {
            var phoneNumber = (string)value["phoneNumber"];
            var code = (string)value["phoneCode"];
            if (string.IsNullOrWhiteSpace(phoneNumber) || string.IsNullOrWhiteSpace(code))
            {
                return BadRequest();
            }
            phoneNumber = phoneNumber.Trim();

            var result = await OwinUserManager.ChangePhoneNumberAsync(this.GetUserId(), phoneNumber, code);

            return result.Succeeded
                ? (IHttpActionResult)StatusCode(HttpStatusCode.NoContent)
                : (IHttpActionResult)BadRequest("Failed to verify phone");
        }

        // PUT api/AccountApi/Email
        [Route("Email")]
        public async Task<IHttpActionResult> PutEmail(JObject value)
        {
            var userId = this.GetUserId();
            // To replace a confirmed email with an unconfirmed one is a bed idea. But we have no infrastructure currently to store an unconfirmed email temporarily. 
            if (await OwinUserManager.IsEmailConfirmedAsync(userId))
            {
                return BadRequest("Unable to change a confirmed email address.");
            }

            var email = (string)value["email"];
            if (string.IsNullOrWhiteSpace(email))
            {
                return BadRequest();
            }
            email = email.Trim();

            var result = await OwinUserManager.SetEmailAsync(userId, email);
            if (result.Succeeded)
            {
                var confirmationToken = await OwinUserManager.GenerateEmailConfirmationTokenAsync(userId);
                var queryString = AccountUtils.GetMailLinkQueryString(confirmationToken, userId);
                var host = Request.RequestUri.GetComponents(UriComponents.Host, UriFormat.Unescaped);
                var link = "http://" + host + "/account/confirm-email?" + queryString;
                await EmailUtils.SendVerificationEmailAsync(email, link);
                return StatusCode(HttpStatusCode.NoContent);
            }
            else
            {
                return BadRequest(result.PlainErrorMessage("Failed to change email address."));
            }
        }

        // POST api/AccountApi/Forgot
        [AllowAnonymous]
        [Route("Forgot")]
        public async Task<IHttpActionResult> PostForgot(JObject value)
        {
            var email = (string)value["email"];
            if (!string.IsNullOrEmpty(email))
            {
                var user = await OwinUserManager.FindByEmailAsync(email.Trim());
                if (user != null)
                {
                    if (await OwinUserManager.IsEmailConfirmedAsync(user.Id))
                    {
                        // Send the link.
                        var token = await OwinUserManager.GeneratePasswordResetTokenAsync(user.Id);
                        var queryString = AccountUtils.GetMailLinkQueryString(token, user.Id);
                        var host = Request.RequestUri.GetComponents(UriComponents.Host, UriFormat.Unescaped);
                        var link = "https://" + host + "/account/reset-password?" + queryString;
                        await this.SendPasswordResetEmailAsync(email, link);
                    }
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
            int userId = AccountUtils.GetUserIdFromQueryStringValue(time);
            if (string.IsNullOrEmpty(code) || userId == 0)
            {
                return BadRequest();
            }
            var password = (string)value["password"];
            var result = await OwinUserManager.ResetPasswordAsync(userId, code, password);
            return result.Succeeded
                ? StatusCode(HttpStatusCode.NoContent)
                : (IHttpActionResult)BadRequest(result.PlainErrorMessage());
        }

        // POST api/AccountApi/PaymentToTeacher
        [AllowAnonymous]
        [Route("PaymentToTeacher")]
        public async Task<IHttpActionResult> PostPaymentToTeacher(JObject value)
        {
            // Revalidate the sender's password.
            var password = ((string)value["password"]);
            var user = await OwinUserManager.FindAsync(this.GetUserName(), password);
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

        // DELETE: api/AccountApi/ExternalLogin
        [Route("ExternalLogin")]
        public async Task<IHttpActionResult> DeleteExternalLogin([FromUri] string loginProvider, [FromUri] string providerKey)
        {
            var result = await OwinUserManager.RemoveLoginAsync(this.GetUserId(), new UserLoginInfo(loginProvider, providerKey));
            if (result.Succeeded)
            {
                var user = await OwinUserManager.FindByIdAsync(this.GetUserId());
                if (user != null)
                {
                    await new LoginHelper(OwinUserManager, OwinAuthenticationManager).Sigin(user, false);
                }
            }
            else
            {
                throw new Exception(result.PlainErrorMessage("Failed to remove external login"));
            }
            return StatusCode(HttpStatusCode.NoContent);
        }







    } // end of class
}
