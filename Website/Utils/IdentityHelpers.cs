using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Owin;
using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.SqlServer;
using System.Runtime.Remoting.Messaging;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

/* The following code is based on +https://aspnet.codeplex.com/SourceControl/latest#Samples/Identity . Description at +http://blogs.msdn.com/b/webdev/archive/2013/12/20/announcing-preview-of-microsoft-aspnet-identity-2-0-0-alpha1.aspx
 * Original file is Models/IdentityModels.cs
 */

namespace Runnymede.Website.Utils
{
    public static class AppClaimTypes
    {
        public const string DisplayName = "englc.com/DisplayName";
        public const string IsTeacher = "englc.com/IsTeacher";
    }

    // You can add User data for the user by adding more properties to your User class, please visit +http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser<int, CustomUserLogin, CustomUserRole, CustomUserClaim>
    {
        public Task<ClaimsIdentity> GenerateUserIdentityAsync(ApplicationUserManager manager)
        {
            return Task.FromResult(GenerateUserIdentity(manager));
        }

        public ClaimsIdentity GenerateUserIdentity(ApplicationUserManager manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = manager.CreateIdentity<ApplicationUser, int>(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class CustomRole : IdentityRole<int, CustomUserRole>
    {
        public CustomRole() { }
        public CustomRole(string name) { Name = name; }
    }
    public class CustomUserRole : IdentityUserRole<int> { }
    public class CustomUserClaim : IdentityUserClaim<int> { }
    public class CustomUserLogin : IdentityUserLogin<int> { }

    [DbConfigurationType(typeof(AppDbConfiguration))] // AppDbConfiguration provides SuspendExecutionStrategy
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, CustomRole, int, CustomUserLogin, CustomUserRole, CustomUserClaim>
    {
        public ApplicationDbContext()
            : base("DefaultConnection")
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }

    public class CustomUserStore : UserStore<ApplicationUser, CustomRole, int, CustomUserLogin, CustomUserRole, CustomUserClaim>
    {
        public CustomUserStore(ApplicationDbContext context)
            : base(context)
        {
        }
    }
    public class CustomRoleStore : RoleStore<CustomRole, int, CustomUserRole>
    {
        public CustomRoleStore(ApplicationDbContext context)
            : base(context)
        {
        }
    }

    public class ApplicationUserManager : UserManager<ApplicationUser, int>
    {
        private DbContext context;

        public ApplicationUserManager()
            : this(new CustomUserStore(new ApplicationDbContext()))
        {
        }

        public ApplicationUserManager(IUserStore<ApplicationUser, int> store)
            : base(store)
        {
            this.context = (store as CustomUserStore).Context;
        }

        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options)
        {
            var manager = new ApplicationUserManager(new CustomUserStore(options.Context.GetDbContext() as ApplicationDbContext));
            manager.UserValidator = new UserValidator<ApplicationUser, int>(manager)
            {
                AllowOnlyAlphanumericUserNames = false, // We use email as user name. By default AllowOnlyAlphanumericUserNames is true.
                RequireUniqueEmail = true
            };
            manager.PasswordValidator = new MinimumLengthValidator(7);
            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.PasswordResetTokens = new DataProtectorTokenProvider(dataProtectionProvider.Create("PasswordReset"));
                manager.UserConfirmationTokens = new DataProtectorTokenProvider(dataProtectionProvider.Create("ConfirmUser"));
            }
            return manager;
        }

        public DbContext Context
        {
            get
            {
                return this.context;
            }
        }
    } // end of class

    public static class OwinExtensions
    {
        public static IAppBuilder UseDbContextFactory(this IAppBuilder app, Func<DbContext> createCallback)
        {
            if (app == null)
            {
                throw new ArgumentNullException("app");
            }
            if (createCallback == null)
            {
                throw new ArgumentNullException("createCallback");
            }

            app.Use(typeof(IdentityFactoryMiddleware<DbContext, IdentityFactoryOptions<DbContext>>),
                new IdentityFactoryOptions<DbContext>()
                {
                    Provider = new IdentityFactoryProvider<DbContext>()
                    {
                        OnCreate = (options) => createCallback()
                    }
                });
            return app;
        }

        public static DbContext GetDbContext(this IOwinContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            return context.Get<DbContext>();
        }
    } // end of class

    public class AppDbConfiguration : DbConfiguration
    {
        public AppDbConfiguration()
        {
            // Enable connection resilience.
            // The Connection Resiliency feature does not work with user-initiated transactions. See Limitations with Retrying Execution Strategies (EF6 onwards). +http://msdn.microsoft.com/en-us/data/dn307226.aspx
            this.SetExecutionStrategy("System.Data.SqlClient", () =>
                SuspendExecutionStrategy
                ? (IDbExecutionStrategy)new DefaultExecutionStrategy()
                : new SqlAzureExecutionStrategy()
                );
        }

        public static bool SuspendExecutionStrategy
        {
            get
            {
                return (bool?)CallContext.LogicalGetData("SuspendExecutionStrategy") ?? false;
            }
            set
            {
                CallContext.LogicalSetData("SuspendExecutionStrategy", value);
            }
        }
    } // end of class

} // end of namespace

#region Helpers
namespace Runnymede.Website.Utils
{
    public static class IdentityHelper
    {
        // Used for XSRF when linking external logins
        public const string XsrfKey = "XsrfId";

        public static void SignIn(ApplicationUserManager manager, ApplicationUser user, bool isPersistent)
        {
            IAuthenticationManager authenticationManager = HttpContext.Current.GetOwinContext().Authentication;
            authenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
            var identity = manager.CreateIdentity(user, DefaultAuthenticationTypes.ApplicationCookie);
            authenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = isPersistent }, identity);
        }

        public const string ProviderNameKey = "providerName";
        public static string GetProviderNameFromRequest(HttpRequest request)
        {
            return request.QueryString[ProviderNameKey];
        }

        public const string CodeKey = "code";
        public static string GetCodeFromRequest(HttpRequestBase request)
        {
            return request.QueryString[CodeKey];
        }

        public const string UserIdKey = "time"; // Originally "userId". Security through obscurity :(
        private const string skip32Key = "yh5bvgcwew"; // Key must be 10 bytes long.

        public static string GetMailLinkQueryString(string code, int userId)
        {
            // 32-bit block cipher. +https://github.com/eleven41/Eleven41.Skip32
            var cipher = new Eleven41.Skip32.Skip32Cipher(Encoding.ASCII.GetBytes(skip32Key));
            var encrypted = cipher.Encrypt(userId);
            // Convert to UInt32 to avoid a sign.
            var bytes = BitConverter.GetBytes(encrypted);
            var unsigned = BitConverter.ToUInt32(bytes, 0);
            var encryptedString = unsigned.ToString();
            return CodeKey + "=" + HttpUtility.UrlEncode(code) + "&" + UserIdKey + "=" + HttpUtility.UrlEncode(encryptedString);
        }

        public static int GetUserIdFromRequest(HttpRequestBase request)
        {
            var queryStringValue = HttpUtility.UrlDecode(request.QueryString[UserIdKey]);
            return GetUserIdFromQueryStringValue(queryStringValue);
        }

        public static int GetUserIdFromQueryStringValue(string queryStringValue)
        {
            uint unsigned;
            if (UInt32.TryParse(queryStringValue, out unsigned))
            {
                var bytes = BitConverter.GetBytes(unsigned);
                var encrypted = BitConverter.ToInt32(bytes, 0);
                var cipher = new Eleven41.Skip32.Skip32Cipher(Encoding.ASCII.GetBytes(skip32Key));
                return cipher.Decrypt(encrypted);
            }
            else
                return 0;
        }

        private static bool IsLocalUrl(string url)
        {
            return !string.IsNullOrEmpty(url) && ((url[0] == '/' && (url.Length == 1 || (url[1] != '/' && url[1] != '\\'))) || (url.Length > 1 && url[0] == '~' && url[1] == '/'));
        }

        //public static void RedirectToReturnUrl(string returnUrl, HttpResponse response)
        //{
        //    if (!String.IsNullOrEmpty(returnUrl) && IsLocalUrl(returnUrl))
        //    {
        //        response.Redirect(returnUrl);
        //    }
        //    else
        //    {
        //        response.Redirect("~/");
        //    }
        //}
        
        public static int GetUserId(IIdentity identity)
        {
            return Convert.ToInt32(identity.GetUserId());
        }

        public static string GetUserName(IIdentity identity)
        {
            return identity.GetUserName();
        }

        public static string GetUserDisplayName(IIdentity identity)
        {
            var claimsIdentity = identity as System.Security.Claims.ClaimsIdentity;
            return claimsIdentity != null ? claimsIdentity.FindFirstValue(AppClaimTypes.DisplayName) : null; // FindFirstValue() returns null if not found.
        }

        public static bool GetUserIsTeacher(IIdentity identity)
        {
            var claimsIdentity = identity as System.Security.Claims.ClaimsIdentity;
            return (claimsIdentity != null) && claimsIdentity.HasClaim(i => i.Type == AppClaimTypes.IsTeacher);
        }
    }
}
#endregion
