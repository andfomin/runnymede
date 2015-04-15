using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Runnymede.Website.Models;
using System;
using System.Data.Entity.SqlServer;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using Runnymede.Common.Utils;
using System.Web.Hosting;
using System.Data.SqlClient;
using System.Data.Entity;
using System.Data;

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

        /// <summary>
        /// Creates a new user and writes it to the database. Creates avatars. Sends email.
        /// </summary>
        /// <param name="model">Comes from the manually filled form. May be null</param>
        /// <param name="loginInfo">Comes from external provider, like Facebook or Google. May be null</param>
        /// <param name="extId">Ideally extId is assigned from the ExtIdCookieName cookie value. It may be null</param>
        /// <param name="currentRequestUri">It is used to determine the server address (production/development) where the confirmation link should point</param>
        /// <returns></returns>
        public async Task<ApplicationUser> Signup(SignupBindingModel model, ExternalLoginInfo loginInfo, string extId, Uri currentRequestUri)
        {
            var user = await InternalSignup(model, loginInfo, extId);

            var error = InspectErrorAfterSignup(user);
            if (String.IsNullOrEmpty(error))
            {
                // We await here. Otherwise error: A second operation started on this context before a previous asynchronous operation completed. Use 'await' to ensure that any asynchronous operations have completed before calling another method on this context.
                var confirmationToken = await this.userManager.GenerateEmailConfirmationTokenAsync(user.Id);
                // Send confirmation. Do not send in the background to avoid showing the confirmation page if the email actually failed.
                var taskEmail = SendConfirmationEmail(user.Email, user.Id, model.Name, confirmationToken, currentRequestUri);
                // Sign in the user.
                var taskSignin = Sigin(user, false);
                // Create a large and a small identicons and save them into the blob storage. 
                var taskAvatarLarge = AccountUtils.CreateAndSaveIdenticonAsync(user.Id, AccountUtils.AvatarLargeSize, AzureStorageUtils.ContainerNames.AvatarsLarge);
                var taskAvatarSmall = AccountUtils.CreateAndSaveIdenticonAsync(user.Id, AccountUtils.AvatarSmallSize, AzureStorageUtils.ContainerNames.AvatarsSmall);
                // Perform the tasks in parallel.    
                await Task.WhenAll(taskEmail, taskSignin, taskAvatarLarge, taskAvatarSmall);
            }

            return user;
        }

        private async Task<ApplicationUser> InternalSignup(SignupBindingModel model, ExternalLoginInfo loginInfo, string extId)
        {
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

            if ((String.IsNullOrEmpty(password) && loginInfo == null))
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
                    /* We do not use this.userManager within the Connection Resiliency block. 
                     * The DbContext should be constructed within the code block to be retried. This ensures that we are starting with a clean state for each retry. 
                     */
                    using (var dbContext = new ApplicationDbContext())
                    using (var userStore = new CustomUserStore(dbContext))
                    using (var userManager = new ApplicationUserManager(userStore))
                    {
                        // Reuse the original validators.
                        userManager.UserValidator = this.userManager.UserValidator;
                        userManager.PasswordValidator = this.userManager.PasswordValidator;

                        /* dbo.appGetNewUserId() generates random Ids and tests them against dbo.aspnetUsers until it finds a vacant one.
                         * Yes, fragmentation in the table increases (it would be somewhat anyway due to editing), bad splits on insert ( but not every time after a page has initially been split in halves).
                         * I believe that after the initial page split many next inserts and edits cause a next split not soon. 
                         * So the only harm is about 1/4 extra space (not doubled, because we continue filling pages randomly up until the default fillfactor.)                         
                         * But the space havily depends on what users put in nvarchars anyway, so to worry about the space is pointless.
                         * Clustered index seeks shouldn't be affected. Splits affect only leaf B-tree pages, branch pages have default fillfactor less than 100% always.
                         * The benefit is we get rid of ExtId nchar(12) in many denormalized tables.
                         * Just to remind, the initial idea behind the original ExtId was to prevent a bad guy from guessing sequecial Ids and download all avatars (avatars are accessable directly as Blobs).
                         * So we sent the true Id to the client plus a separate ExtId for avatar URL and we stored it denormalized in many places.
                         * Now ExtId is just a pre-signup cookie value not used anywhere else.
                         */
                        user.Id = await dbContext.Database.SqlQuery<int>("select dbo.appGetNewUserId();").SingleAsync();

                        using (var transaction = dbContext.Database.BeginTransaction())
                        {
                            // First phase. Create an ASP.NET Identity user in dbo.aspnetUsers
                            var identityResult = String.IsNullOrWhiteSpace(password)
                                 ? await userManager.CreateAsync(user)
                                 : await userManager.CreateAsync(user, password);

                            if (identityResult.Succeeded)
                            {
                                // Second phase. dbo.appUsers. Postpone the creation of the money account until it is really used.
                                await dbContext.Database.ExecuteSqlCommandAsync(
                                    "insert dbo.appUsers (Id, DisplayName, ExtId) values (@p0, @p1, @p2);",
                                    user.Id, displayName, extId);

                                /* The UserManager checks the existence of the username in another connection. 
                                 * So in the case of an external authorisation (Google, Facebook) AddLoginAsync() does not see the username created by CreateAsync() in our transaction. 
                                 * Using IsolationLevel.ReadUncommitted for our transaction would not help since the UserManager reads with ReadCommitted. 
                                 * So we are forced to commit prematurely.
                                 */
                                transaction.Commit();
                            }
                            else
                            {
                                transaction.Rollback();
                            }

                            if (identityResult.Succeeded && loginInfo != null)
                            {
                                identityResult = await userManager.AddLoginAsync(user.Id, loginInfo.Login);
                            }

                            if (!identityResult.Succeeded)
                            {
                                // A hack to report the error to the caller method. See InspectErrorAfterSignup().
                                user.Id = 0;
                                var dummyClaim = new CustomUserClaim
                                {
                                    ClaimType = ClaimTypes.UserData,
                                    ClaimValue = identityResult.PlainErrorMessage("Signup failed.")
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
        /// Send the confirmation link in background
        /// </summary>
        /// <param name="email"></param>
        /// <param name="userId"></param>
        /// <param name="confirmationToken"></param>
        /// <param name="currentRequestUri">It is used to determine the server address (production/development) where the confirmation link should point</param>
        public async Task SendConfirmationEmail(string email, int userId, string displayName, string confirmationToken, Uri currentRequestUri)
        //public void SendConfirmationEmailInBackground(string email, int userId, string displayName, string confirmationToken, Uri currentRequestUri)
        {
            var queryString = AccountUtils.GetMailLinkQueryString(confirmationToken, userId);
            var host = currentRequestUri.GetComponents(UriComponents.Host, UriFormat.Unescaped);
            var link = "http://" + host + "/account/confirm-email?" + queryString;
            // TODO If the task fails, we will lose the message. The proper solution is to place the message into a reliable queue.
            //HostingEnvironment.QueueBackgroundWorkItem(ct => EmailUtils.SendConfirmationEmailAsync(email, displayName, link));
            await EmailUtils.SendConfirmationEmailAsync(email, displayName, link);
        }

        public static string CoalesceDisplayName(string name)
        {
            return String.IsNullOrWhiteSpace(name) ? "Anonymous" : name.Trim();
        }

    }


}