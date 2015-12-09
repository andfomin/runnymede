using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Runnymede.Common.Utils;
using Runnymede.Website.Models;
using Runnymede.Website.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Linq;
using System.Xml.Linq;
using System.Data;

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
                return owinUserManager ?? (owinUserManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>());
            }
        }

        private IAuthenticationManager owinAuthenticationManager;
        private IAuthenticationManager OwinAuthenticationManager
        {
            get
            {
                return owinAuthenticationManager ?? (owinAuthenticationManager = HttpContext.GetOwinContext().Authentication);
            }
        }

        // GET: /account/signup
        [RequireHttps]
        [AllowAnonymous]
        public ActionResult Signup(string returnUrl)
        {
            // Don't log the user out. Even if she has been authenticated with password, she can manually return here to login with an extrnal login.
            if (Request.IsAuthenticated)
            {
                return RedirectToManageLoginsPage();
            }
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // GET: /account/thank-you
        [AllowAnonymous]
        public ActionResult ThankYou()
        {
            return View();
        }

        // GET: /account/confirm-email?code=qwertyuiop&time=123456
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail()
        {
            int userId = AccountUtils.GetUserIdFromRequest(Request);
            string code = AccountUtils.GetCodeFromRequest(Request);
            bool success = false;
            if (userId != 0 && !String.IsNullOrEmpty(code))
            {
                var result = await OwinUserManager.ConfirmEmailAsync(userId, code);
                success = result.Succeeded;
            }
            if (success && Request.IsAuthenticated)
            {
                return Redirect(Url.Action("Edit", "Account", null, "https") + "#/logins");
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
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { returnUrl = returnUrl }));
        }

        private ActionResult RedirectToManageLoginsPage(string error = null)
        {
            // Clear any partial cookies from external sign ins
            OwinAuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
            var queryString = String.IsNullOrEmpty(error)
                ? ""
                : "?error=" + Uri.EscapeUriString(error); // Uri.Encode replaces spaces with '+', not '%20'
            return Redirect(Url.Action("Edit", "Account", null, "https") + "#/logins" + queryString);
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
                ViewBag.ReturnUrl = returnUrl;
                var model = new ExternalLoginBindingModel
                {
                    Email = loginInfo.Email,
                    Name = loginInfo.ExternalIdentity.Name
                };
                return View("ExternalLoginConfirmation", model);
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
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginBindingModel model, string returnUrl)
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
            var user = await loginHelper.Signup(signupModel, loginInfo, this.GetExtId(), Request.Url);

            var error = loginHelper.InspectErrorAfterSignup(user);
            if (String.IsNullOrEmpty(error))
            {
                if (String.IsNullOrEmpty(returnUrl))
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    return RedirectToLocal(returnUrl);
                }
            }
            else
            {
                ModelState.AddModelError("", error);
                //return View("Signup");
                return View("ExternalLoginConfirmation", model);
            }
        }

        // GET: /account/balance/
        [RequireHttps]
        public ActionResult Balance(PayPalPaymentResult payPalPaymentResult = PayPalPaymentResult.None)
        {
            return View(payPalPaymentResult);
        }

        // GET: /account/transactions
        [RequireHttps]
        public ActionResult Transactions(PayPalPaymentResult payPalPaymentResult = PayPalPaymentResult.None)
        {
            return View(payPalPaymentResult);
        }

        //        // GET: /account/buy-reviews
        //        [RequireHttps]
        //        [AllowAnonymous]
        //        public async Task<ActionResult> BuyReviews()
        //        {
        //            var sql = @"
        //select Title, Price from dbo.appGetPricelistItems() order by Position;
        //select dbo.appGetValue('PLDSCT');
        //";
        //            dynamic pricelist = null;
        //            await DapperHelper.QueryMultipleResilientlyAsync(sql, null, CommandType.Text,
        //                (Dapper.SqlMapper.GridReader reader) =>
        //                {
        //                    var items = reader.Read<dynamic>();

        //                    var value = reader.Read<string>().Single();
        //                    // Convert XML to JSON and pass to the client only a subset of data. There may be sensitive data in XML.
        //                    var discounts = XElement.Parse(value)
        //                        .Elements("Percent")
        //                        .Select(i => new
        //                        {
        //                            amountFrom = Convert.ToDecimal(i.Attribute("amountFrom").Value),
        //                            amountTo = Convert.ToDecimal(i.Attribute("amountTo").Value),
        //                            percent = Convert.ToDecimal(i.Value),
        //                        })
        //                        ;

        //                    pricelist = new
        //                    {
        //                        Items = items,
        //                        Discounts = discounts,
        //                    };
        //                });

        //            ViewBag.Pricelist = pricelist;
        //            return View();
        //        }

        // GET: /account/buy-services
        [RequireHttps]
        [AllowAnonymous]
        public async Task<ActionResult> BuyServices()
        {
            var sql = @"
select Title, Price from dbo.appGetPricelistItems() order by Position;
";
            var items = await DapperHelper.QueryResilientlyAsync<dynamic>(sql);
            ViewBag.Pricelist = items;
            return View();
        }

        // GET: /account/edit/
        [RequireHttps]
        public ActionResult Edit()
        {
            return View();
        }

        // GET: /account/forgot-password/
        [AllowAnonymous]
        [RequireHttps]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        // GET: /account/reset-password?code=qwertyuiop
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
        // GET: /account/paypal-payment
        public ActionResult PaypalPayment(string tx = null)
        {
            PayPalPaymentResult result = PayPalPaymentResult.Canceled;

            var helper = new PayPalHelper();

            var txnId = !String.IsNullOrEmpty(tx)
                // tx in PDT from Sandbox is upper-case on automatic return and lower-case on manual return. A PDT request returns error 4002 if given a low-case tx.
                ? tx.ToUpper()
                : "PDT " + KeyUtils.GetCurrentTimeKey();

            helper.WriteLog(PayPalLogEntity.NotificationKind.PDT, txnId, Request.Url.Query);

            if (!String.IsNullOrEmpty(tx))
            {
                // Query PayPal.
                var response = helper.RequestPaymentDetails(txnId);

                var logRowKey = helper.WriteLog(PayPalLogEntity.NotificationKind.DetailsRequest, txnId, response);

                // Parse the response
                var lines = helper.SplitPdtMessage(response);
                var payment = helper.ParsePaymentLines(lines);

                // Write the payment to the database.
                if (helper.PostIncomingPayPalPayment(payment, logRowKey))
                {
                    result = PayPalPaymentResult.Success;
                }
            }

            return RedirectToAction("Transactions", new { PayPalPaymentResult = result });
        }

        // GET: /account/paypal-ipn
        [AllowAnonymous]
        [RequireHttps]
        public ActionResult PaypalIpn()
        {
            var content = Request.BinaryRead(Request.ContentLength);
            var message = Encoding.ASCII.GetString(content);

            var helper = new PayPalHelper();

            var lines = helper.SplitIpnMessage(message);
            var payment = helper.ParsePaymentLines(lines);
            var txnId = !String.IsNullOrEmpty(payment.TxnId) ? payment.TxnId : "IPN " + KeyUtils.GetCurrentTimeKey();

            var logRowKey = helper.WriteLog(PayPalLogEntity.NotificationKind.IPN, txnId, message);

            var response = helper.VerifyIPN(message, txnId);

            helper.WriteLog(PayPalLogEntity.NotificationKind.IPNResponse, txnId, response);

            if (response == "VERIFIED")
            {
                if (helper.PostIncomingPayPalPayment(payment, logRowKey))
                {
                    //helper.PurchaseOnIncomingPayPalPayment(payment);
                }
            }

            return new HttpStatusCodeResult(HttpStatusCode.NoContent);
        }

        // POST: /account/upload-avatar
        //[RequireHttps] // Called from the Edit page which is loaded over HTTPS
        //[HttpPost]
        //public async Task<ActionResult> UploadAvatar(HttpPostedFileBase fileInput)
        //{
        //    if (fileInput != null && fileInput.ContentLength > 0)
        //    {
        //        // ResizeAndSaveAvatar() will rewind the stream to the beginning. Do not parallelize the processing with Task.WhenAll().
        //        var stream = fileInput.InputStream;
        //        var blobName = KeyUtils.IntToKey(this.GetUserId());
        //        await UploadUtils.ResizeAndSaveAvatar(stream, AccountUtils.AvatarSmallSize, AzureStorageUtils.ContainerNames.AvatarsSmall, blobName);
        //        await UploadUtils.ResizeAndSaveAvatar(stream, AccountUtils.AvatarLargeSize, AzureStorageUtils.ContainerNames.AvatarsLarge, blobName);
        //    }
        //    //return new HttpStatusCodeResult(HttpStatusCode.NoContent); // ngUpload misses the end event until it has received a content back.
        //    // IE wants text/html not application/json. However ngUpload passes it to ng-upload as a parsed JSON, i.e. Object {}.
        //    return Content("{}", "text/html");
        //}

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

        // GET: /account/support-sso/
        [RequireHttps]
        public ActionResult SupportSso()
        {
            const string pathTemplate = "http://support.englisharium.com/login/sso?name={0}&email={1}&timestamp={2}&hash={3}";
            var key = ConfigurationManager.AppSettings["FreshdeskSsoKey"];

            var name = this.GetUserDisplayName();
            var email = this.GetUserName();

            string timems = Convert.ToInt64((DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds).ToString();
            var hash = GetHash(key, name, email, timems);
            var path = String.Format(pathTemplate, HttpUtility.UrlEncode(name), HttpUtility.UrlEncode(email), timems, hash);

            return Redirect(path);
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

        private static string GetHash(string secret, string name, string email, string timems)
        {
            string input = name + email + timems;
            var keybytes = Encoding.Default.GetBytes(secret);
            var inputBytes = Encoding.Default.GetBytes(input);

            var crypto = new HMACMD5(keybytes);
            byte[] hash = crypto.ComputeHash(inputBytes);

            StringBuilder sb = new StringBuilder();
            foreach (byte b in hash)
            {
                string hexValue = b.ToString("X").ToLower(); // Lowercase for compatibility on case-sensitive systems
                sb.Append((hexValue.Length == 1 ? "0" : "") + hexValue);
            }
            return sb.ToString();
        }

        #endregion

    }
}