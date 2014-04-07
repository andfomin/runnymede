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
using System.Threading.Tasks;

namespace Runnymede.Website.Controllers.Mvc
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
                DoSignout();
                return RedirectToAction("Signin");
            }

            return View();
        }

        // GET: account/confirm?code=qwertyuiop&time=123456
        [AllowAnonymous]
        [RequireHttps]
        public ActionResult Confirm()
        {
            DoSignout();

            int userId = IdentityHelper.GetUserIdFromRequest(Request);
            string code = IdentityHelper.GetCodeFromRequest(Request);
            bool success = false;
            if (userId != 0 && !string.IsNullOrEmpty(code))
            {
                var result = this.OwinUserManager.ConfirmUser<ApplicationUser, int>(userId, code);
                success = result.Succeeded;
            }

            //return Redirect(Url.Action("Signin", this.GetControllerName(), new { confirmed = success }, "https"));
            return RedirectToAction("Signin", new { confirmed = success });
        }

        // GET: /account/signin
        [AllowAnonymous]
        [RequireHttps]
        public ActionResult Signin(string returnUrl)
        {
            DoSignout();
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // GET: /account/signout
        [AllowAnonymous]
        public ActionResult Signout()
        {
            DoSignout();
            return Redirect(Url.Action("Signin", this.GetControllerName(), null, "https"));
            //The script in the HeadScripts section of the page deletes the access token from the browser's local storage. Then the script redirects to the Login page.
            //return View();
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

        private ApplicationUserManager OwinUserManager
        {
            get { return Request.GetOwinContext().GetUserManager<ApplicationUserManager>(); }
        }

        private void DoSignout()
        {
            // Delete the authentication cookie. 
            Request.GetOwinContext().Authentication.SignOut();
        }
    }
}