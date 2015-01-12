using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Owin;
using Runnymede.Website.Utils;
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
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.SqlClient;

/* The following code is based on +https://aspnet.codeplex.com/SourceControl/latest#Samples/Identity . Description at +http://blogs.msdn.com/b/webdev/archive/2013/12/20/announcing-preview-of-microsoft-aspnet-identity-2-0-0-alpha1.aspx
 * Original file is Models/IdentityModels.cs
 */

namespace Runnymede.Website.Models
{

    public static class AppClaimTypes
    {
        public const string DisplayName = "englisharium.com/DisplayName"; // We don't use System.Security.Claims.ClaimTypes.Name because of its length and to save on traffic costs.
        public const string IsTeacher = "englisharium.com/IsTeacher";
    }

    public class ApplicationUser : IdentityUser<int, CustomUserLogin, CustomUserRole, CustomUserClaim>
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(ApplicationUserManager manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);

            // Add custom user claims
            const string sql = @"
select DisplayName, IsTeacher from dbo.appGetUser(@Id)
";
            var appUser = (await DapperHelper.QueryResilientlyAsync<dynamic>(sql, new { Id = this.Id })).SingleOrDefault();

            if (appUser != null)
            {
                var displayName = (string)appUser.DisplayName;
                var isTeacher = (bool?)appUser.IsTeacher;

                if (!String.IsNullOrWhiteSpace(displayName))
                {
                    userIdentity.AddClaim(new Claim(AppClaimTypes.DisplayName, displayName));
                }

                if (isTeacher.HasValue && isTeacher.Value)
                {
                    // Mere presense or absence matters.
                    userIdentity.AddClaim(new Claim(AppClaimTypes.IsTeacher, ""));
                }
            }

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

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Since Id is marked with [Key] attribute, Entity Framework expects an autoincremented column. We provide a custom value for Id. Tell EF to send our value on insert.
            modelBuilder
                .Entity<ApplicationUser>()
                .Property(i => i.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
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
        public ApplicationUserManager()
            : this(new CustomUserStore(new ApplicationDbContext()))
        {
        }

        public ApplicationUserManager(IUserStore<ApplicationUser, int> store)
            : base(store)
        {
            //this.dbContext = (store as CustomUserStore).Context;
        }

        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context)
        {
            var dbContext = context.Get<ApplicationDbContext>();
            var userStore = new CustomUserStore(dbContext);
            var manager = new ApplicationUserManager(userStore);

            // Configure validation logic for usernames
            manager.UserValidator = new UserValidator<ApplicationUser, int>(manager)
            {
                AllowOnlyAlphanumericUserNames = false, // We use email as user name. By default AllowOnlyAlphanumericUserNames is true.
                RequireUniqueEmail = true
            };

            // Configure validation logic for passwords
            manager.PasswordValidator = new MinimumLengthValidator(6);

            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider = new DataProtectorTokenProvider<ApplicationUser, int>(dataProtectionProvider.Create("additional entropy gv#UJ7"));
            }

            return manager;
        }
    } // end of class ApplicationUserManager

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
    } // end of class AppDbConfiguration

} // end of namespace

