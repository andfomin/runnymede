using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.Owin.Security.OAuth;
using Owin;
using Runnymede.Website.Utils;
using System;

namespace Runnymede.Website
{
    public partial class Startup
    {

        static Startup()
        {
            PublicClientId = "self";

            //UserManagerFactory = () => new UserManager<IdentityUser>(new UserStore<IdentityUser>());
            UserManagerFactory = () =>
            {
                return new ApplicationUserManager();
            };

            OAuthOptions = new OAuthAuthorizationServerOptions
            {
                TokenEndpointPath = new PathString("/Token"),
                Provider = new ApplicationOAuthProvider(PublicClientId, UserManagerFactory),
                AuthorizeEndpointPath = new PathString("/api/AccountApi/ExternalLogin"),
                AccessTokenExpireTimeSpan = TimeSpan.FromDays(14 + 1), // AF. Longer than the persistant cookie to prevent a situation when a page is opening but "not working".
                AllowInsecureHttp = true
            };
        }

        public static OAuthAuthorizationServerOptions OAuthOptions { get; private set; }

        public static Func<ApplicationUserManager> UserManagerFactory { get; set; }

        public static string PublicClientId { get; private set; }


        // For more information on configuring authentication, please visit +http://go.microsoft.com/fwlink/?LinkId=301883
        public void ConfigureAuth(IAppBuilder app)
        {
            app.UseDbContextFactory(ApplicationDbContext.Create);

            // Configure the UserManager
            app.UseUserManagerFactory(new IdentityFactoryOptions<ApplicationUserManager>()
            {
                DataProtectionProvider = app.GetDataProtectionProvider(),
                Provider = new IdentityFactoryProvider<ApplicationUserManager>()
                {
                    OnCreate = ApplicationUserManager.Create
                }
            });

            // Enable the application to use a cookie to store information for the signed in user
            // and to use a cookie to temporarily store information about a user logging in with a third party login provider
            // Configure the sign in cookie
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/account/signin"),
                CookieSecure = CookieSecureOption.Never,
                SlidingExpiration = false, // I do not know how to re-issue the complimented bearer token automaticaly.
                Provider = new CookieAuthenticationProvider
                {
                    OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<ApplicationUserManager, ApplicationUser, int>(
                        validateInterval: TimeSpan.FromMinutes(20),
                        regenerateIdentityCallback: (manager, user) => user.GenerateUserIdentityAsync(manager),
                        getUserIdCallback: (id) => (Int32.Parse(id.GetUserId())))
                }
            });

            // Use a cookie to temporarily store information about a user logging in with a third party login provider
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);


            /* SAD STORY. I tryed to use [HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)] in ApiControllers. 
             * But the problem is HTML5's localStorage has the strict same-domain policy. A bearer token stored in HTTPS is not available in HTTP.
               Workarounds are very hacky and involve extra round-trips. 
               So I have surrendered to [HostAuthentication(DefaultAuthenticationTypes.ApplicationCookie)] in ApiControllers.
             */
            // Enable the application to use bearer tokens to authenticate users
            //app.UseOAuthBearerTokens(new OAuthAuthorizationServerOptions
            //{
            //    TokenEndpointPath = new PathString("/Token"),
            //    Provider = new ApplicationOAuthProvider(PublicClientId, UserManagerFactory),
            //    AuthorizeEndpointPath = new PathString("/api/AccountApi/ExternalLogin"),
            //    AccessTokenExpireTimeSpan = TimeSpan.FromDays(14 + 1), // AF. Longer than the persistant cookie to prevent a situation when a page is opening but "not working".
            //    AllowInsecureHttp = true
            //});
            app.UseOAuthBearerTokens(OAuthOptions);

            // Uncomment the following lines to enable logging in with third party login providers
            //app.UseMicrosoftAccountAuthentication(
            //    clientId: "",
            //    clientSecret: "");

            //app.UseTwitterAuthentication(
            //   consumerKey: "",
            //   consumerSecret: "");

            //app.UseFacebookAuthentication(
            //   appId: "",
            //   appSecret: "");

            //app.UseGoogleAuthentication();
        }
    }
}
