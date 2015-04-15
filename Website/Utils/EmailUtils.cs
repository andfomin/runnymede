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
using System.Configuration;
using Newtonsoft.Json;
using System.Net.Http;

namespace Runnymede.Website.Utils
{
    public static class EmailUtils
    {
        public static async Task SendConfirmationEmailAsync(string email, string displayName, string link)
        {
            var helper = new MandrillHelper();
            await helper.SendConfirmationEmailAsync(email, displayName, link);
        }

        public static async Task SendVerificationEmailAsync(string email, string displayName, string link)
        {
            var helper = new MandrillHelper();
            await helper.SendVerificationEmailAsync(email, displayName, link);
        }

        public static async Task SendPasswordResetEmailAsync(string email, string link)
        {
            var helper = new MandrillHelper();
            await helper.SendPasswordResetEmailAsync(email, link);
        }

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
                message.From = new MailAddress("do-not-reply@englisharium.com", "Englisharium");
                message.To.Add(new MailAddress(to));
                //message.Bcc.Add(new MailAddress("ca6477711715@gmail.com")); // Duplicate the message.

                // Subject and multipart/alternative Body
                message.Subject = subject;
                message.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(textBody, null, MediaTypeNames.Text.Plain));
                message.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(htmlBody, null, MediaTypeNames.Text.Html));

                // Init SmtpClient. Read the host and the port from Web.config. <system.net><mailSettings><smtp><network>
                using (var smtpClient = new SmtpClient())
                {
                    var userName = ConfigurationManager.AppSettings["SmtpUserName"];
                    var password = ConfigurationManager.AppSettings["SmtpPassword"];
                    smtpClient.Credentials = new System.Net.NetworkCredential(userName, password);

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

        public static async Task SendConfirmationEmailAsync0(string email, string displayName, string link)
        {
            string subject = "=?utf-8?Q?=E2=98=85?= Please confirm your registration"; // + " Second reminder."
            //string subject = "=?utf-8?Q?=E2=98=91?= Please confirm your email address"; // U+2611: ballot box with check // + " Second reminder."

            string textBodyTemplate =
@"Thank you for joining Englisharium!
Please go to the following link to confirm your email address:
{0}
";
            string textyBody = string.Format(textBodyTemplate, link);

            string htmlBodyTemplate = @"<html><body>Thank you for joining Englisharium!</br>
Please click the following link to confirm your email address<br/><a href=""{0}"">{0}</a>
</body></html>";
            string htmlBody = string.Format(htmlBodyTemplate, link);

            await SendEmailAsync(email, subject, textyBody, htmlBody);
        }

        public static async Task SendPasswordResetEmailAsync0(this ApiController controller, string email, string link)
        {
            string subject = "=?utf-8?Q?=E2=98=91?= Password reset request";

            string textBodyTemplate =
@"Please go to the following link to reset your password for Englisharium.
{0}";
            string textyBody = string.Format(textBodyTemplate, link);

            string htmlBodyTemplate = @"<html><body>Please click the following link to reset your password for Englisharium</br>
<a href=""{0}"">{0}</a></body></html>";
            string htmlBody = string.Format(htmlBodyTemplate, link);

            await SendEmailAsync(email, subject, textyBody, htmlBody);
        }

        public static async Task SendVerificationEmailAsync0(string email, string link)
        {
            string subject = "=?utf-8?Q?=E2=98=85?= Please confirm your email address"; // + " Second reminder."
            //string subject = "=?utf-8?Q?=E2=98=91?= Please confirm your email address"; // U+2611: ballot box with check // + " Second reminder."

            string textBodyTemplate =
@"Thank you for participating at Englisharium!
Please go to the following link to confirm your email address:
{0}
";
            string textyBody = string.Format(textBodyTemplate, link);

            string htmlBodyTemplate = @"<html><body>Thank you for participating at Englisharium!</br>
Please click the following link to confirm your email address<br/><a href=""{0}"">{0}</a>
</body></html>";
            string htmlBody = string.Format(htmlBodyTemplate, link);

            await SendEmailAsync(email, subject, textyBody, htmlBody);
        }
    }

    public class MandrillHelper
    {
        /* 
         * +https://mandrillapp.com/api/docs/messages.JSON.html#method=send-template
         * Sample code showing how to use the Mandrill API +https://gist.github.com/andyhuey/3444063         
         */

        private const string MandrillBaseUrl = "https://mandrillapp.com/api/1.0/";
        private const string ApiMethodPath = "messages/send-template.json";

        private class EmailAddress
        {
            [JsonProperty("email")]
            public string Email { get; set; }
            ///// <summary>
            ///// the optional display name to use for the recipient
            ///// </summary>
            //[JsonProperty("name")]
            //public string Name { get; set; }
            ///// <summary>
            ///// The header type to use for the recipient, defaults to "to" if not provided. oneof(to, cc, bcc)
            ///// </summary>
            //[JsonProperty("type")]
            //public string Type { get; set; }
        }

        private class TemplateContent
        {
            [JsonProperty("name")]
            public string Name { get; set; }
            [JsonProperty("content")]
            public string Content { get; set; }
        }

        private class MessageInfo
        {
            [JsonProperty("to")]
            public IEnumerable<EmailAddress> To { get; set; }
            [JsonProperty("track_opens")]
            public bool TrackOpens { get; set; }
            [JsonProperty("track_clicks")]
            public bool TrackClicks { get; set; }
            [JsonProperty("global_merge_vars")]
            public IEnumerable<TemplateContent> GlobalMergeVars { get; set; }
            [JsonProperty("tags")]
            public IEnumerable<string> Tags { get; set; }
        }

        private class MandrillMessage
        {
            [JsonProperty("key")]
            public string Key { get; set; }
            [JsonProperty("template_name")]
            public string TemplateName { get; set; }
            /// <summary>
            /// Required. May be null.
            /// </summary>
            [JsonProperty("template_content")]
            public IEnumerable<TemplateContent> TemplateContent { get; set; }
            [JsonProperty("message")]
            public MessageInfo Message { get; set; }
        }

        private async Task PostMessage(string templateName, MessageInfo messageInfo)
        {
            var key = ConfigurationManager.AppSettings["MandrillApiKey"];
            var value = new MandrillMessage
            {
                Key = key,
                TemplateName = templateName,
                Message = messageInfo,
            };

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(MandrillBaseUrl);
                var response = await client.PostAsJsonAsync(ApiMethodPath, value);
                if (!response.IsSuccessStatusCode){
                    var content = await response.Content.ReadAsStringAsync();
                    throw new Exception(content);
                }
            }
        }

        public async Task SendConfirmationEmailAsync(string toEmail, string displayName, string link)
        {
            var templateName = "signup-with-trio-image";

            var messageInfo = new MessageInfo
            {
                To = new[] {
                    new EmailAddress {
                        Email =  toEmail,                    
                    },
                },
                TrackOpens = true,
                TrackClicks = false,
                GlobalMergeVars = new[] { 
                    new TemplateContent {
                        Name = "DISPLAY_NAME",
                        Content = displayName,
                    },
                    new TemplateContent {
                        Name = "CONFIRMATION_LINK",
                        Content = link,
                    },
                },
                Tags = new[] { "signup_confirmation" },
            };

            await PostMessage(templateName, messageInfo);
        }

        public async Task SendVerificationEmailAsync(string toEmail, string displayName, string link)
        {
            var templateName = "email-verification";

            var messageInfo = new MessageInfo
            {
                To = new[] {
                    new EmailAddress {
                        Email =  toEmail,                    
                    },
                },
                TrackOpens = true,
                TrackClicks = false,
                GlobalMergeVars = new[] { 
                    new TemplateContent {
                        Name = "DISPLAY_NAME",
                        Content = displayName,
                    },
                    new TemplateContent {
                        Name = "CONFIRMATION_LINK",
                        Content = link,
                    },
                },
                Tags = new[] { "email_verification" },
            };

            await PostMessage(templateName, messageInfo);
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string link)
        {
            var templateName = "reset-password";

            var messageInfo = new MessageInfo
            {
                To = new[] {
                    new EmailAddress {
                        Email =  toEmail,                    
                    },
                },
                TrackOpens = false,
                TrackClicks = false,
                GlobalMergeVars = new[] { 
                    new TemplateContent {
                        Name = "RESET_LINK",
                        Content = link,
                    },
                },
                Tags = new[] { "reset_password" },
            };

            await PostMessage(templateName, messageInfo);
        }





    }



}