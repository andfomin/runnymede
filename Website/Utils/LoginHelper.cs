using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Runnymede.Website.Models;
using System;
using System.Data.Entity.SqlServer;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;

namespace Runnymede.Website.Utils
{
    public class LoginHelper
    {

        private ApplicationUserManager userManager;
        private IAuthenticationManager authManager;

        public LoginHelper(ApplicationUserManager userManager, IAuthenticationManager authManager)
        {
            this.userManager = userManager;
            this.authManager = authManager;
        }

        public async Task<ApplicationUser> Signup(SignupBindingModel model, ExternalLoginInfo loginInfo, Guid keeper, Uri currentRequestUri)
        {
            // Guid ExtId is used to avoid direct Id-to-icon realtionship and prevent a bad guy from downloading all user photos in bulk.
            // Ideally extId is assigned the Keeper cookie value.
            var extId = keeper != Guid.Empty ? keeper : Guid.NewGuid();

            var user = await InternalSignup(model, loginInfo, extId);

            var error = InspectErrorAfterSignup(user);
            if (string.IsNullOrEmpty(error))
            {
                // We await here. Otherwise error: A second operation started on this context before a previous asynchronous operation completed. Use 'await' to ensure that any asynchronous operations have completed before calling another method on this context.
                var confirmationToken = await this.userManager.GenerateEmailConfirmationTokenAsync(user.Id);
                // Send the confirmation link.
                var taskEmail = SendConfirmationEmailAsync(user.Email, user.Id, confirmationToken, currentRequestUri);

                // Sign in the user.
                var taskSignin = Sigin(user, false);

                // Create a large and a small identicons and save them into the blob storage. 
                // Each icon consists of 6 x 6 blocks (5 blocks pattern + 2 x 1/2 margin). So sizes below are 6 x 20 = 120 and 6 x 6 = 36 pixels.
                var taskAvatarLarge = AccountUtils.CreateAndSaveIdenticonAsync(extId, AccountUtils.AvatarLargeSize / 6, AzureStorageUtils.ContainerNames.AvatarsLarge);
                var taskAvatarSmall = AccountUtils.CreateAndSaveIdenticonAsync(extId, AccountUtils.AvatarSmallSize / 6, AzureStorageUtils.ContainerNames.AvatarsSmall);

                // Perform the long-running tasks in parallel.    
                await Task.WhenAll(taskEmail, taskSignin, taskAvatarLarge, taskAvatarSmall);
            }

            return user;
        }

        private async Task<ApplicationUser> InternalSignup(SignupBindingModel model, ExternalLoginInfo loginInfo, Guid extId)
        {
            IdentityResult result;
            string email = null;
            string name = null;
            string password = null;

            // ExternalLoginInfo comes from an external authentication provider, i.e. Facebook or Google.
            if (loginInfo != null)
            {
                email = loginInfo.Email;
                name = loginInfo.ExternalIdentity.Name;
            }
            // SignupBindingModel comes filled out manually by the user. Override values from the external service with the manually entered values.
            if (model != null)
            {
                email = model.Email;
                name = model.Name;
                password = model.Password;
            }

            if ((string.IsNullOrEmpty(password) && loginInfo == null) || (extId == Guid.Empty))
            {
                throw new ArgumentNullException();
            }

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
            };

            var displayName = CoalesceDisplayName(name);

            // The internal Connection Resiliency strategy does not work with user-initiated transactions.
            // The workaround is described in "Limitations with Retrying Execution Strategies" +http://msdn.microsoft.com/en-us/data/dn307226.aspx
            AppDbConfiguration.SuspendExecutionStrategy = true; // try-finally is not needed. The value is stored per request.

            var executionStrategy = new SqlAzureExecutionStrategy();

            await executionStrategy.Execute(
                async () =>
                {
                    // We do not use this.userManager within the Connection Resiliency block.
                    // The DbContext should be constructed within the code block to be retried. This ensures that we are starting with a clean state for each retry.
                    var dbContext = new ApplicationDbContext();
                    using (var userManager = new ApplicationUserManager(new CustomUserStore(dbContext)))
                    {
                        // Reuse the original validators.
                        userManager.UserValidator = this.userManager.UserValidator;
                        userManager.PasswordValidator = this.userManager.PasswordValidator;

                        using (var transaction = dbContext.Database.BeginTransaction())
                        {
                            // First phase. Create an ASP.NET Identity user.
                            result = string.IsNullOrWhiteSpace(password)
                                ? await this.userManager.CreateAsync(user)
                                : await this.userManager.CreateAsync(user, password);

                            if (result.Succeeded && loginInfo != null)
                            {
                                result = await this.userManager.AddLoginAsync(user.Id, loginInfo.Login);
                            }

                            if (result.Succeeded)
                            {
                                // Second phase. dbo.appUsers and accounting. user.Id is auto-generated in dbo.aspnetUsers.
                                await dbContext.Database.ExecuteSqlCommandAsync(
                                    "execute dbo.appCreateUserAndAccounts @UserId = @p0, @DisplayName = @p1, @ExtId = @p2",
                                    user.Id, displayName, extId);

                                transaction.Commit();
                            }
                            else
                            {
                                transaction.Rollback();

                                // A hack to report the error to the caller method. See InspectErrorAfterSignup().
                                user.Id = 0;
                                var dummyClaim = new CustomUserClaim
                                {
                                    ClaimType = ClaimTypes.UserData,
                                    ClaimValue = result.PlainErrorMessage("Signup failed.")
                                };
                                user.Claims.Add(dummyClaim);
                            }
                        }
                    }
                });

            AppDbConfiguration.SuspendExecutionStrategy = false;

            return user;
        }

        /// <summary>
        /// Uses a hack to transfer an error message from the Signup() method. Async methods do not allow out params.
        /// </summary>
        /// <param name="user">The user object returned by Signup() </param>
        /// <returns></returns>
        public string InspectErrorAfterSignup(ApplicationUser user)
        {
            string result = null;
            if (user != null && user.Id == 0 && user.Claims.Count > 0)
            {
                result = user.Claims
                    .Where(i => i.ClaimType == ClaimTypes.UserData)
                    .Select(i => i.ClaimValue)
                    .SingleOrDefault();
            }
            return result;
        }

        public async Task Sigin(ApplicationUser user, bool isPersistent)
        {
            // Clear any partial cookies from external sign ins
            this.authManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
            var userIdentity = await user.GenerateUserIdentityAsync(this.userManager);
            this.authManager.SignIn(new AuthenticationProperties { IsPersistent = isPersistent }, userIdentity);
        }

        /// <summary>
        /// Send the confirmation link.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="currentRequestUri">Used to determine the server address where the link should point</param>
        /// <returns></returns>
        public async Task SendConfirmationEmailAsync(string email, int userId, string confirmationToken, Uri currentRequestUri)
        {
            // There is no approved solution for "fire-and-forget" in ASP.NET. The proper solution is to place a request into a reliable queue (such as Azure Queue). See +http://blog.stephencleary.com/2012/12/returning-early-from-aspnet-requests.html
            if (string.IsNullOrEmpty(email))
            {
                var queryString = AccountUtils.GetMailLinkQueryString(confirmationToken, userId);
                var host = currentRequestUri.GetComponents(UriComponents.Host, UriFormat.Unescaped);
                var link = "http://" + host + "/account/confirm-email?" + queryString;
                await EmailUtils.SendConfirmationEmailAsync(email, link);
            }
        }

        public static string CoalesceDisplayName(string name)
        {
            return string.IsNullOrWhiteSpace(name) ? "Anonymous" : name.Trim();
        }

    }


}