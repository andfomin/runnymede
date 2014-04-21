﻿using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Runnymede.Website.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Owin;
using Runnymede.Website.Models;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using Microsoft.AspNet.Identity.Owin;

namespace Runnymede.Website.Controllers.Mvc
{
    [Authorize]
    public class AccountController : Runnymede.Website.Utils.CustomController
    {
        const string UnexpectedExtrnalLoginError = "Please use this page to add an external login to your existing account.";

        private ApplicationUserManager owinUserManager;
        private ApplicationUserManager OwinUserManager
        {
            get
            {
                if (owinUserManager == null)
                {
                    owinUserManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
                }
                return owinUserManager;
            }
        }

        private IAuthenticationManager owinAuthenticationManager;
        private IAuthenticationManager OwinAuthenticationManager
        {
            get
            {
                if (owinAuthenticationManager == null)
                {
                    owinAuthenticationManager = HttpContext.GetOwinContext().Authentication;
                }
                return owinAuthenticationManager;
            }
        }

        // GET: /account/signup
        [RequireHttps]
        [AllowAnonymous]
        public ActionResult Signup()
        {
            // Don't log the user out. Even if she has been authenticated with password, she can manually return here to login with an extrnal login.
            if (Request.IsAuthenticated)
            {
                return RedirectToManageLoginsPage();
            }
            return View();
        }

        // GET: account/confirm-email?code=qwertyuiop&time=123456
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail()
        {
            int userId = IdentityHelper.GetUserIdFromRequest(Request);
            string code = IdentityHelper.GetCodeFromRequest(Request);
            bool success = false;
            if (userId != 0 && !string.IsNullOrEmpty(code))
            {
                var result = await OwinUserManager.ConfirmEmailAsync(userId, code);
                success = result.Succeeded;
            }
            if (success && Request.IsAuthenticated)
            {
                return new RedirectResult(Url.Action("Edit", "Account", null, "https") + "#/personal");
            }
            else
            {
                // If the user comes to the Login page authenticated, she gets redirected further to the login management page.
                OwinAuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                return Redirect(Url.Action("Login", "Account", new { confirmed = success }, "https"));
            }
        }

        // GET: /account/login
        [AllowAnonymous]
        [RequireHttps]
        public ActionResult Login(string returnUrl)
        {
            // Don't log the user out. Even if she has been authenticated with password, she can manually return here to login with an extrnal login.
            if (Request.IsAuthenticated)
            {
                return RedirectToManageLoginsPage();
            }
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // GET: /account/logout
        [AllowAnonymous]
        public ActionResult Logout()
        {
            OwinAuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return Redirect(Url.Action("Login", "Account", null, "https"));
        }

        // POST: /account/external-login
        [HttpPost]
        [AllowAnonymous]
        [RequireHttps]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        private ActionResult RedirectToManageLoginsPage(string error = null)
        {
            // Clear any partial cookies from external sign ins
            OwinAuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
            var queryString = string.IsNullOrEmpty(error)
                ? ""
                : "?error=" + Uri.EscapeUriString(error); // Uri.Encode replaces spaces with '+', not '%20'
            return new RedirectResult(Url.Action("Edit", "Account", null, "https") + "#/logins" + queryString);
        }

        // GET: /account/external-login-callback
        [AllowAnonymous]
        [RequireHttps]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            if (Request.IsAuthenticated)
            {
                return RedirectToManageLoginsPage(UnexpectedExtrnalLoginError);
            }

            var loginInfo = await OwinAuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login", new { error = "external-login-failure" });
            }

            var user = await OwinUserManager.FindAsync(loginInfo.Login);
            if (user != null)
            {
                // The user has an acoount. Sign her in.
                await new LoginHelper(OwinUserManager, OwinAuthenticationManager).Sigin(user, true);
                return RedirectToLocal(returnUrl);
            }
            else
            {
                // If the user does not have an account, then prompt the user to create an account.
                ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                return View("ExternalLoginConfirmation", new ExternalLoginBindingModel { Email = loginInfo.Email, Name = loginInfo.ExternalIdentity.Name });
            }
        }

        //// GET: /account/external-login-callback
        //[AllowAnonymous]
        //[RequireHttps]
        //public async Task<ActionResult> ExternalLoginCallback0(string returnUrl)
        //{
        //    var loginInfo = await OwinAuthenticationManager.GetExternalLoginInfoAsync();
        //    if (loginInfo == null)
        //    {
        //        return RedirectToAction("Login");
        //    }

        //    bool isPersistent = true;
        //    ActionResult result = RedirectToLocal(returnUrl);

        //    var user = await OwinUserManager.FindAsync(loginInfo.Login);
        //    if (user == null)
        //    {
        //        // If the user does not have an account, then create an account
        //        //ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
        //        //return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email, DisplayName = loginInfo.ExternalIdentity.Name });

        //        var signupHelper = new LoginHelper(OwinUserManager, OwinAuthenticationManager);
        //        // Create the user account
        //        user = await signupHelper.Signup(null, loginInfo, this.GetKeeper());
        //        // Send the confirmation link.
        //        await signupHelper.SendConfirmationEmail(user, Request.Url);

        //        isPersistent = false;
        //        result = RedirectToAction("Index", "Home");
        //    }

        //    if (user != null)
        //    {
        //        // Sign in the user with this external login provider if the user already has a login
        //        var userIdentity = await user.GenerateUserIdentityAsync(OwinUserManager);
        //        OwinAuthenticationManager.SignIn(new AuthenticationProperties { IsPersistent = isPersistent }, userIdentity);
        //    }

        //    // Clear any partial cookies from external sign ins
        //    OwinAuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);

        //    return result;
        //}

        // POST: /account/external-login-confirmation
        [HttpPost]
        [AllowAnonymous]
        [RequireHttps]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginBindingModel model)
        {
            // Get the information about the user from the external login provider
            var loginInfo = await OwinAuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login", new { error = "external-login-failure" });
            }
            // Override the external login info with the values supplyed by the user.
            var signupModel = new SignupBindingModel
            {
                Email = model.Email,
                Name = model.Name,
            };

            var loginHelper = new LoginHelper(OwinUserManager, OwinAuthenticationManager);

            // Create the user account
            var user = await loginHelper.Signup(signupModel, loginInfo, this.GetKeeper(), Request.Url);

            var error = loginHelper.InspectErrorAfterSignup(user);
            if (string.IsNullOrEmpty(error))
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ModelState.AddModelError("", error);
                //return View("Signup");
                return View("ExternalLoginConfirmation", model);
            }
        }

        // GET: account/balance/
        [RequireHttps]
        public ActionResult Balance(PayPalPaymentResult payPalPaymentResult = PayPalPaymentResult.None)
        {
            return View(payPalPaymentResult);
        }

        // GET: /account/add-money
        [RequireHttps]
        //public async Task<ActionResult> AddMoney()
        public ActionResult AddMoney()
        {
            //var confirmedEmail = await this.OwinUserManager.IsConfirmedAsync(this.GetUserId());
            return View();
        }

        // GET: /account/pay-teacher/12345?teacher=qwerty
        [RequireHttps]
        public ActionResult PayTeacher(string id, string teacher)
        {
            ViewBag.TeacherUserId = Convert.ToInt32(id);
            ViewBag.TeacherDisplayName = teacher;
            return View();
        }

        // GET: account/edit/
        [RequireHttps]
        public ActionResult Edit()
        {
            return View();
        }

        // GET: account/forgot-password/
        [AllowAnonymous]
        [RequireHttps]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        // GET: account/reset-password?code=qwertyuiop
        [AllowAnonymous]
        [RequireHttps]
        public ActionResult ResetPassword()
        {
            return View();
        }

        public enum PayPalPaymentResult
        {
            None,
            Success,
            Canceled
        }

        // Be carefull with capitalization of the action method name. Route mapper will add a dash.
        // GET: account/paypal-payment?tx=ExtId
        [AllowAnonymous]
        [RequireHttps]
        public ActionResult PaypalPayment(string tx = null)
        {
            PayPalPaymentResult result = PayPalPaymentResult.Canceled;

            if (!string.IsNullOrEmpty(tx))
            {
                var helper = new PayPalHelper();

                helper.WriteLog(PayPalLogEntity.NotificationKind.PDT, tx, Request.Url.Query);

                // Query PayPal.
                var response = helper.RequestPaymentDetails(tx);

                var logRowKey = helper.WriteLog(PayPalLogEntity.NotificationKind.DetailsRequest, tx, response);

                var lines = helper.SplitPdtMessage(response);

                // Write the payment to the database.
                helper.PostIncomingPaymentTransaction(lines, logRowKey);

                if (lines.Count() > 0)
                {
                    result = PayPalPaymentResult.Success;
                }
            }

            return RedirectToAction("Balance", new { PayPalPaymentResult = result });
        }

        // GET: account/ipn/
        [AllowAnonymous]
        [RequireHttps]
        public ActionResult ipn()
        {
            var content = Request.BinaryRead(Request.ContentLength);
            var message = Encoding.ASCII.GetString(content);

            var helper = new PayPalHelper();
            var lines = helper.SplitIpnMessage(message);
            var pairs = helper.SplitKeyValuePairs(lines);

            var tx = pairs.ContainsKey("txn_id") ? pairs["txn_id"] : "IPN " + LoggingUtils.GetUniqueObservedTime();

            var logRowKey = helper.WriteLog(PayPalLogEntity.NotificationKind.IPN, tx, message);

            var response = helper.VerifyIPN(message);

            helper.WriteLog(PayPalLogEntity.NotificationKind.IPNResponse, tx, response);

            if (response == "VERIFIED")
            {
                helper.PostIncomingPaymentTransaction(lines, logRowKey);
            }

            return new HttpStatusCodeResult(HttpStatusCode.NoContent);
        }

        // POST: /account/upload-avatar
        [RequireHttps] // Called from the Edit page which is loaded over HTTPS
        [HttpPost]
        public async Task UploadAvatar(HttpPostedFileBase avatarFile)
        {
            if (avatarFile != null && avatarFile.ContentLength > 0)
            {
                const string sql = @"
select ExtIdUpperCase from dbo.appUsers where Id = @UserId
";
                var blobName = DapperHelper.QueryResiliently<string>(sql, new { UserId = this.GetUserId() })
                    .Single();

                var stream = avatarFile.InputStream;
                // ResizeAndSaveAvatar() will rewind the stream to the beginning.
                await UploadHelper.ResizeAndSaveAvatar(stream, AccountControllerUtils.AvatarSmallSize, AzureStorageUtils.ContainerNames.AvatarsSmall, blobName);
                await UploadHelper.ResizeAndSaveAvatar(stream, AccountControllerUtils.AvatarLargeSize, AzureStorageUtils.ContainerNames.AvatarsLarge, blobName);
            }
        }

        // POST: /account/link-login
        [RequireHttps]
        [HttpPost]
        public ActionResult LinkLogin(string provider)
        {
            // Request a redirect to the external login provider to link a login for the current user
            return new AccountController.ChallengeResult(provider, Url.Action("LinkLoginCallback", "Account"), this.GetUserId().ToString());
        }

        // Not called by the user.
        [RequireHttps]
        public async Task<ActionResult> LinkLoginCallback()
        {
            var loginInfo = await OwinAuthenticationManager.GetExternalLoginInfoAsync(XsrfKey, this.GetUserId().ToString());
            string error = loginInfo != null ? null : "External login failed";
            if (error == null)
            {
                var result = await OwinUserManager.AddLoginAsync(this.GetUserId(), loginInfo.Login);
                error = result.PlainErrorMessage("Failed to link external login");
            }
            return new RedirectResult(Url.Action("Edit", "Account", null, "https") + "#/logins" + (error == null ? "" : "?error=" + Uri.EscapeUriString(error))); // Uri.Encode replaces spaces with '+', not '%20'
        }


        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion





    }
}