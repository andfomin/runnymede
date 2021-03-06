﻿using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.Owin.Security.Facebook;
using Microsoft.Owin.Security.OAuth;
using Owin;
using Runnymede.Common.Utils;
using Runnymede.Website.Models;
using Runnymede.Website.Utils;
using System;
using System.Configuration;
using System.Web.Http;

namespace Runnymede.Website
{
    public partial class Startup
    {
        // For more information on configuring authentication, please visit +http://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app)
        {
            // Configure the db context, user manager and role manager to use a single instance per request
            // The bad thing is that it is called on EVERY request.
            /* There is extension method Owin.MapWhenExtensions.MapWhen() which can separate requests like 
             * app.MapWhen(context => context.Request.Uri.PathAndQuery.StartsWith("/api"), newApp => { var config = new System.Web.Http.HttpConfiguration(); app.UseWebApi(config); });
             */
            app.CreatePerOwinContext<ApplicationDbContext>(ApplicationDbContext.Create); 
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
            //app.CreatePerOwinContext<ApplicationRoleManager>(ApplicationRoleManager.Create); // We don't use user roles. We use exclusively custom claims.

            /* THE SAD STORY. I have tryed to use [HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)] in ApiControllers.
             * 
             * But the problem is HTML5's localStorage has the strict same-domain policy. A bearer token stored in HTTPS is not available in HTTP.
             * Workarounds are very hacky and involve extra round-trips. 
             * 
             * Another problem is that different authentication methods for pages and AJAX may have different expire time and cause the situation when a page opens but "is not working".
             * 
             * Session cookie and sessionStorage work differently across tabs. See +http://dev.w3.org/html5/webstorage/#introduction
             * If the user opens a new tab, she is still authorized to view the page, but AJAX requests from that page will fail.
             * 
             * So I have surrendered to [HostAuthentication(DefaultAuthenticationTypes.ApplicationCookie)] in ApiControllers.
             */

            // Enable the application to use a cookie to store information for the signed in user
            // and to use a cookie to temporarily store information about a user logging in with a third party login provider
            // Configure the sign in cookie
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/account/login"),
                Provider = new CookieAuthenticationProvider
                {
                    // Enables the application to validate the security stamp when the user logs in.
                    // This is a security feature which is used when you change a password or add an external login to your account.  
                    //OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<ApplicationUserManager, ApplicationUser, int>(
                    //    validateInterval: TimeSpan.FromMinutes(30),
                    //    regenerateIdentityCallback: (manager, user) => user.GenerateUserIdentityAsync(manager),
                    //    getUserIdCallback: (identity) => Convert.ToInt32(identity.GetUserId())),
                },
                CookieSecure = CookieSecureOption.Never,
            });

            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            // Uncomment the following lines to enable logging in with third party login providers
            //app.UseMicrosoftAccountAuthentication(
            //    clientId: "",
            //    clientSecret: "");

            //app.UseTwitterAuthentication(
            //   consumerKey: "",
            //   consumerSecret: "");

            /* +http://www.asp.net/mvc/overview/security/create-an-aspnet-mvc-5-app-with-facebook-and-google-oauth2-and-openid-sign-on
             * Lookup for "Creating a Google app for OAuth 2 and connecting the app to the project" +https://console.developers.google.com/
             * Lookup for "Creating the app in Facebook and connecting the app to the project" +https://developers.facebook.com/apps            
             */
            var facebookOptions = new FacebookAuthenticationOptions()
            {
                AppId = ConfigurationManager.AppSettings["FacebookOAuthAppId"],
                AppSecret = ConfigurationManager.AppSettings["FacebookOAuthAppSecret"],
            };
            facebookOptions.Scope.Add("email");
            facebookOptions.Scope.Add("public_profile");
            app.UseFacebookAuthentication(facebookOptions);

            app.UseGoogleAuthentication(
                clientId: ConfigurationManager.AppSettings["GoogleOAuthClientId"],
                clientSecret: ConfigurationManager.AppSettings["GoogleOAuthClientSecret"]
            );
        }
    }
}
