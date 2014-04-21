using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web.Http;
using System.Net.Mime;
using System.Net.Configuration;

namespace Runnymede.Website.Utils
{
    public static class EmailUtils
    {
        /*
        <system.net>
          <mailSettings>
            <smtp from="test@domain.com">
              <network host="smtp.sendgrid.net" port="587" userName="username" password="password" />
            </smtp>
          </mailSettings>
        </system.net> 
        */

        ////private static SmtpClient GetMailtrapSmtpClient()
        ////{
        ////    var smtpClient = new SmtpClient("mailtrap.io", 2525);
        ////    smtpClient.Credentials = new NetworkCredential("ex01-61488ae444161cbd", "8818c540431b02fd");
        ////    return smtpClient;
        ////}

        ////private static SmtpClient GetSendGridSmtpClient()
        ////{
        ////    //var smtpClient = new SmtpClient("smtp.sendgrid.net", 587);
        ////    //smtpClient.Credentials = new System.Net.NetworkCredential("neonlamp", "L2mplamp");
        ////    //smtpClient.Credentials = new System.Net.NetworkCredential("azure_118d51984693c99ba52f8762f623195b@azure.com", "7hhsvdpp");
        ////    var smtpClient = new SmtpClient();
        ////    return smtpClient;
        ////}

        private static async Task SendEmailAsync(string to, string subject, string textBody, string htmlBody)
        {
            try
            {
                var message = new MailMessage();
                message.From = new MailAddress("do-not-reply@englc.com", "English Cosmos");
                message.To.Add(new MailAddress(to));
                //message.Bcc.Add(new MailAddress("ca6477711715@gmail.com")); // Duplicate the message.

                // Subject and multipart/alternative Body
                message.Subject = subject;
                message.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(textBody, null, MediaTypeNames.Text.Plain));
                message.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(htmlBody, null, MediaTypeNames.Text.Html));

                // Init SmtpClient
                ////var smtpClient = textBody.Contains("//localhost") ? GetMailtrapSmtpClient() : GetSendGridSmtpClient();
                // Read settings from Web.config
                using (var smtpClient = new SmtpClient())
                {
                    await smtpClient.SendMailAsync(message);
                }
            }
            catch (SmtpException smtpEx)
            {
                //Error handling here
                throw smtpEx;
            }
            catch (Exception ex)
            {
                //Error handling here
                throw ex;
            }
        }

        public static async Task SendConfirmationEmailAsync(string email, string link)
        {
            string subject = "=?utf-8?Q?=E2=98=85?= Please confirm your registration"; // + " Second reminder."
            //string subject = "=?utf-8?Q?=E2=98=91?= Please confirm your email address"; // U+2611: ballot box with check // + " Second reminder."

            string textBodyTemplate =
@"Thank you for registering with English Cosmos!
Please go to the following link to confirm your email address:
{0}
";
            string textyBody = string.Format(textBodyTemplate, link);

            string htmlBodyTemplate = @"<html><body>Thank you for registering with English Cosmos!</br>
Please click the following link to confirm your email address<br/><a href=""{0}"">{0}</a>
</body></html>";
            string htmlBody = string.Format(htmlBodyTemplate, link);

            await SendEmailAsync(email, subject, textyBody, htmlBody);
        }

        public static async Task SendPasswordResetEmailAsync(this ApiController controller, string email, string link)
        {
            string subject = "=?utf-8?Q?=E2=98=91?= Password reset request";

            string textBodyTemplate =
@"Please go to the following link to reset your password for English Cosmos.
{0}";
            string textyBody = string.Format(textBodyTemplate, link);

            string htmlBodyTemplate = @"<html><body>Please click the following link to reset your password for English Cosmos</br>
<a href=""{0}"">{0}</a></body></html>";
            string htmlBody = string.Format(htmlBodyTemplate, link);

            await SendEmailAsync(email, subject, textyBody, htmlBody);
        }

        public static async Task SendVerificationEmailAsync(string email, string link)
        {
            string subject = "=?utf-8?Q?=E2=98=85?= Please confirm your email address"; // + " Second reminder."
            //string subject = "=?utf-8?Q?=E2=98=91?= Please confirm your email address"; // U+2611: ballot box with check // + " Second reminder."

            string textBodyTemplate =
@"Thank you for using English Cosmos!
Please go to the following link to confirm your email address:
{0}
";
            string textyBody = string.Format(textBodyTemplate, link);

            string htmlBodyTemplate = @"<html><body>Thank you for using English Cosmos!</br>
Please click the following link to confirm your email address<br/><a href=""{0}"">{0}</a>
</body></html>";
            string htmlBody = string.Format(htmlBodyTemplate, link);

            await SendEmailAsync(email, subject, textyBody, htmlBody);
        }





    }
}