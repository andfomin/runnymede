using Microsoft.AspNet.Identity;
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

namespace Runnymede.Community.Controllers
{
    [Authorize]
    public class AccountController : Runnymede.Website.Utils.CustomController
    {

        // GET: /account/create
        [RequireHttps]
        [AllowAnonymous]
        public ActionResult Create()
        {
            if (Request.IsAuthenticated)
            {
                Request.GetOwinContext().Authentication.SignOut();
            }

            return View();
        }

        // GET: account/confirm?code=qwertyuiop&time=123456
        [AllowAnonymous]
        public ActionResult Confirm()
        {
            string code = IdentityHelper.GetCodeFromRequest(Request);
            int userId = IdentityHelper.GetUserIdFromRequest(Request);
            bool success = false;
            if (!string.IsNullOrEmpty(code) && userId != 0)
            {
                var manager = Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
                var result = manager.ConfirmUser<ApplicationUser, int>(userId, code);
                success = result.Succeeded;
            }
            //return RedirectToAction("Signin", new { confirmed = success });
            return Redirect(Url.Action("Signin", this.GetControllerName(), new { confirmed = success }, "https"));
        }

        // GET: /account/signin
        [AllowAnonymous]
        [RequireHttps]
        public ActionResult Signin(string returnUrl)
        {
            if (Request.IsAuthenticated)
            {
                Request.GetOwinContext().Authentication.SignOut();
            }

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // GET: /account/signout
        [AllowAnonymous]
        public ActionResult Signout()
        {
            // Delete the authentication cookie. 
            //Ver 1.0. Request.GetOwinContext().Authentication.SignOut(CookieAuthenticationDefaults.AuthenticationType);
            Request.GetOwinContext().Authentication.SignOut();
            //The script in the HeadScripts section of the page deletes the access token from the browser's local storage. Then the script redirects to the Login page.
            //return View();
            return Redirect(Url.Action("Signin", this.GetControllerName(), null, "https"));
        }

        // GET: account/balance/
        [RequireHttps]
        public ActionResult Balance()
        {
            return View();
        }

        // GET: /account/add-money
        [RequireHttps]
        public ActionResult AddMoney()
        {
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
        public ActionResult ResetPassword()
        {
            return View();
        }

        // Be carefull with capitalization of the action method name. Route mapper will add a dash.
        // GET: account/paypal-payment?tx=ExtId
        [AllowAnonymous]
        [RequireHttps]
        public ActionResult PaypalPayment(string tx = null)
        {
            if (!string.IsNullOrEmpty(tx))
            {
                var helper = new PayPalHelper();

                helper.WriteLog(PayPalLogEntity.NotificationKind.PDT, tx, Request.Url.Query);

                // Query PayPal.
                var response = helper.RequestPaymentDetails(tx);
                var logRowKey = helper.WriteLog(PayPalLogEntity.NotificationKind.DetailsRequest, tx, response);

                var lines = helper.SplitPdtMessage(response);

                helper.PostPaymentTransaction(lines, logRowKey);

                if (lines.Count() > 0)
                {
                    return View("PaypalSuccess");
                }
            }

            return View("PaypalCancel");
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
                helper.PostPaymentTransaction(lines, logRowKey);
            }

            return new HttpStatusCodeResult(HttpStatusCode.NoContent);
        }

        private ApplicationUserManager OwinUserManager
        {
            get { return Request.GetOwinContext().GetUserManager<ApplicationUserManager>(); }
        }
    }
}