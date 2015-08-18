using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json.Linq;
using Runnymede.Common.Utils;
using Runnymede.Website.Models;
using Runnymede.Website.Utils;
using System;
using System.Collections;
using System.Data;
using System.Data.Entity.SqlServer;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using System.Xml.Linq;

namespace Runnymede.Website.Controllers.Api
{
    [Authorize]
    //[HostAuthentication(DefaultAuthenticationTypes.ApplicationCookie)]
    [RoutePrefix("api/accounts")]
    public class AccountsApiController : ApiController
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

        // DELETE: api/accounts/external_login
        [Route("external_login")]
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

        // GET api/accounts/personal_profile
        [Route("personal_profile")]
        public async Task<IHttpActionResult> GetPersonalProfile()
        {
            var sql = @"
select U.Id, U.DisplayName, U.IsTeacher, U.SkypeName, U.Announcement, AU.Email, AU.EmailConfirmed
from dbo.appGetUser(@Id) U
	inner join dbo.aspnetGetUser(@Id) AU on U.Id = AU.Id;
";
            var result = (await DapperHelper.QueryResilientlyAsync<dynamic>(sql, new { Id = this.GetUserId() })).Single();
            return Ok(result);
        }

        // GET api/accounts/teacher_profile
        [Route("teacher_profile")]
        public async Task<IHttpActionResult> GetTeacherProfile()
        {
            var sql = @"
select U.Id, U.IsTeacher, U.SessionRate, AU.PhoneNumber, AU.PhoneNumberConfirmed
from dbo.appGetUser(@Id) U
	inner join dbo.aspnetGetUser(@Id) AU on U.Id = AU.Id;
";
            var result = (await DapperHelper.QueryResilientlyAsync<dynamic>(sql, new { Id = this.GetUserId() })).Single();
            return Ok(result);
        }

        // GET api/accounts/logins
        [Route("logins")]
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
            //var hasPassword = (user != null) && !String.IsNullOrEmpty(user.PasswordHash);

            var result = new
            {
                UserLogins = userLogins,
                OtherLogins = otherLogins,
                HasPassword = hasPassword,
            };

            return Ok(result);
        }

        // GET /api/accounts/transactions?offset=0&limit=10
        [Route("transactions")]
        public async Task<IHttpActionResult> GetTransactions(int offset, int limit)
        {
            var entries = await DapperHelper.QueryPageItems<BalanceEntryDto>("dbo.accGetTransactions", new
            {
                UserId = this.GetUserId(),
                RowOffset = offset,
                RowLimit = limit
            });
            return Ok(entries);
        }

        // GET api/accounts/presentation/2000000001
        [Route("presentation/{id}")]
        public async Task<IHttpActionResult> GetPresentation(int id)
        {
            // TODO. Enable CORS on the Azure blob and download presentation directly using AJAX. +http://odetocode.com/blogs/scott/archive/2014/03/31/http-clients-and-azure-blob-storage.aspx
            // +http://blog.cynapta.com/2013/12/cynapta-azure-cors-helper-free-tool-to-manage-cors-rules-for-windows-azure-blob-storage/
            var blob = AzureStorageUtils.GetBlob(AzureStorageUtils.ContainerNames.Presentations, KeyUtils.IntToKey(id));
            var text = await blob.DownloadTextAsync();
            //var text = await AzureStorageUtils.GetBlobAsText(AzureStorageUtils.ContainerNames.Presentations, KeyUtils.IntToKey(id));
            return new RawStringResult(this, text, RawStringResult.TextMediaType.PlainText);
        }

        // POST api/accounts/signup
        [Route("signup")]
        [AllowAnonymous]
        public async Task<IHttpActionResult> PostSignup(SignupBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var loginHelper = new LoginHelper(OwinUserManager, OwinAuthenticationManager);

            // Create the user account
            var user = await loginHelper.Signup(model, null, this.GetExtId(), Request.RequestUri);

            var error = loginHelper.InspectErrorAfterSignup(user);
            if (!String.IsNullOrEmpty(error))
            {
                throw new Exception(error);
            }

            // Send the link to the thankyou page. The client code will redirect.
            var url = Url.Link("Default", new { Controller = "Account", Action = "ThankYou", email = model.Email });
            url = url.Replace("https://", "http://"); // Url.Link appears to generate a url using the scheme of the current request. 
            return Ok(url);
            //return StatusCode(HttpStatusCode.Created);
        }

        // POST api/accounts/login
        [AllowAnonymous]
        [Route("login")]
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

        // POST api/accounts/phone_number
        [Route("phone_number")]
        public async Task<IHttpActionResult> PostPhoneNumber(JObject value)
        {
            var phoneNumber = (string)value["phoneNumber"];
            string code = null;
            if (!String.IsNullOrEmpty(phoneNumber))
            {
                code = await OwinUserManager.GenerateChangePhoneNumberTokenAsync(this.GetUserId(), phoneNumber.Trim());
            }
            if (!String.IsNullOrEmpty(code))
            {
                return StatusCode(HttpStatusCode.NoContent);
                // Do not return the code to the client while in production! Send it via SMS.
                //return Ok(code); 
            }
            else
                return BadRequest("Verification code not sent.");

            // A phone number can be removed by calling UserManager.SetPhoneNumberAsync(userId, null);
        }

        // POST api/accounts/phone_verification
        [Route("phone_verification")]
        public async Task<IHttpActionResult> PostPhoneVerification(JObject value)
        {
            var phoneNumber = (string)value["phoneNumber"];
            var code = (string)value["phoneCode"];
            if (String.IsNullOrWhiteSpace(phoneNumber) || String.IsNullOrWhiteSpace(code))
            {
                return BadRequest();
            }
            phoneNumber = phoneNumber.Trim();

            var result = await OwinUserManager.ChangePhoneNumberAsync(this.GetUserId(), phoneNumber, code);

            return result.Succeeded
                ? (IHttpActionResult)StatusCode(HttpStatusCode.NoContent)
                : (IHttpActionResult)BadRequest("Failed to verify phone");
        }

        // POST api/accounts/forgot
        [AllowAnonymous]
        [Route("forgot")]
        public async Task<IHttpActionResult> PostForgot(JObject value)
        {
            var email = (string)value["email"];
            if (!String.IsNullOrEmpty(email))
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
                        await EmailUtils.SendPasswordResetEmailAsync(email, link);
                    }
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST api/accounts/Reset
        [AllowAnonymous]
        [Route("reset")]
        public async Task<IHttpActionResult> PostReset([FromUri] string code, [FromUri] string time, [FromBody] JObject value)
        {
            //string code = IdentityHelper.GetCodeFromRequest(Request);
            int userId = AccountUtils.GetUserIdFromQueryStringValue(time);
            if (String.IsNullOrEmpty(code) || userId == 0)
            {
                return BadRequest();
            }
            var password = (string)value["password"];
            var result = await OwinUserManager.ResetPasswordAsync(userId, code, password);
            return result.Succeeded
                ? StatusCode(HttpStatusCode.NoContent)
                : (IHttpActionResult)BadRequest(result.PlainErrorMessage());
        }

        // PUT api/accounts/profile
        [Route("profile")]
        public IHttpActionResult PutProfile([FromBody] JObject value)
        {
            // Update only changed fields. The null value indicates that the field has not been changed.
            var displayName = (string)value["displayName"];
            if (displayName != null)
            {
                displayName = LoginHelper.CoalesceDisplayName(displayName);
            }

            // If a parameter is null, that meens keep the corresponding field intact.
            DapperHelper.ExecuteResiliently("dbo.appUpdateUser", new
            {
                UserId = this.GetUserId(),
                DisplayName = displayName,
                SkypeName = (string)value["skypeName"],
            },
            CommandType.StoredProcedure);

            return StatusCode(HttpStatusCode.NoContent);
        }

        // PUT api/accounts/announcement
        [Route("announcement")]
        public IHttpActionResult PutAnnouncement([FromBody] JObject value)
        {
            var announcement = (string)value["announcement"];
            DapperHelper.ExecuteResiliently("dbo.appUpdateUser",
                new
                {
                    UserId = this.GetUserId(),
                    Announcement = announcement,
                },
                CommandType.StoredProcedure);
            return StatusCode(HttpStatusCode.NoContent);
        }

        // PUT api/accounts/presentation
        [Route("presentation")]
        public async Task<IHttpActionResult> PutPresentation([FromBody] JObject value)
        {
            var text = (string)value["presentation"];
            if (text != null)
            {
                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(text)))
                {
                    var blobName = KeyUtils.IntToKey(this.GetUserId());
                    await AzureStorageUtils.UploadBlobAsync(stream, AzureStorageUtils.ContainerNames.Presentations, blobName, "text/plain; charset=utf-8");
                    // CloudBlockBlob.UploadTextAsync() writes ContentType "application/octet-stream".
                }
            }
            return StatusCode(text != null ? HttpStatusCode.NoContent : HttpStatusCode.BadRequest);
        }

        // PUT api/accounts/password
        [Route("password")]
        public async Task<IHttpActionResult> PutPassword(JObject value)
        {
            var oldPassword = (string)value["oldPassword"];
            var newPassword = (string)value["newPassword"];
            var userId = this.GetUserId();

            var result = String.IsNullOrEmpty(oldPassword)
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

        // PUT api/accounts/email
        [Route("email")]
        public async Task<IHttpActionResult> PutEmail(JObject value)
        {
            var userId = this.GetUserId();
            // TODO. Store the old confirmed email. To replace a confirmed email with an unconfirmed one is a bed idea. But we have no infrastructure currently to store an unconfirmed email temporarily. 
            if (await OwinUserManager.IsEmailConfirmedAsync(userId))
            {
                return BadRequest("Unable to change a confirmed email address.");
            }

            var email = (string)value["email"];
            if (String.IsNullOrWhiteSpace(email))
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
                var displayName = this.GetUserDisplayName();
                await EmailUtils.SendVerificationEmailAsync(email, displayName, link);
                return StatusCode(HttpStatusCode.NoContent);
            }
            else
            {
                return BadRequest(result.PlainErrorMessage("Failed to change email address."));
            }
        }

        private async Task InternalPutTextIntoBlob(string text, string containerName)
        {
            if (text != null)
            {
                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(text)))
                {
                    var blobName = KeyUtils.IntToKey(this.GetUserId());
                    await AzureStorageUtils.UploadBlobAsync(stream, containerName, blobName, "text/plain; charset=utf-8");
                }
            }
        }

    } // end of class
}
