using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Xml.Linq;
using Dapper;
using Runnymede.Website.Models;
using Runnymede.Common.Utils;
using System.Net.Http;
using System.Text;
using System.Configuration;

namespace Runnymede.Website.Utils
{
    public class PayPalHelper
    {
        public const string PayPalUserNameHashSalt = "#Salt7B190B1B03B9#";

#if DEBUG
        private const string PayPalAddress = "https://www.sandbox.paypal.com/cgi-bin/webscr";
        private const string ReceiverEmail = "paypal-test-seller-usd%40englisharium.com";
#else
        private const string PayPalAddress = "https://www.paypal.com/cgi-bin/webscr";
        private const string ReceiverEmail = "paypal%40englisharium.com";
#endif

        public string WriteLog(PayPalLogEntity.NotificationKind kind, string tx, string logData)
        {
            if (String.IsNullOrEmpty(tx))
            {
                throw new ArgumentException();
            }

            var rowKey = KeyUtils.GetCurrentTimeKey();

            var entity = new PayPalLogEntity
            {
                PartitionKey = tx,
                RowKey = rowKey,
                Kind = kind.ToString(),
                LogData = logData,
            };
            AzureStorageUtils.InsertEntity(AzureStorageUtils.TableNames.PaymentLog, entity);

            return rowKey;
        }

        //public string RequestPaymentDetails0(string tx)
        //{
        //    HttpWebRequest req = (HttpWebRequest)WebRequest.Create(PayPalAddress);

        //    //Set values for the request
        //    req.Method = "POST";
        //    req.ContentType = "application/x-www-form-urlencoded";

        //    string query = "cmd=_notify-synch&tx=" + tx + "&at=" + PayPalAuthToken;
        //    req.ContentLength = query.Length;
        //    using (var streamOut = new StreamWriter(req.GetRequestStream(), System.Text.Encoding.ASCII))
        //    {
        //        streamOut.Write(query);
        //    }

        //    //Send the request to PayPal and get the response
        //    var webResponse = req.GetResponse();
        //    string response;
        //    using (var streamIn = new StreamReader(webResponse.GetResponseStream()))
        //    {
        //        response = streamIn.ReadToEnd();
        //    }

        //    return response;
        //}

        //public string VerifyIPN0(string message, string tx = null)
        //{
        //    //Post back
        //    var req = (HttpWebRequest)WebRequest.Create(PayPalAddress);
        //    req.Method = "POST";
        //    req.ContentType = "application/x-www-form-urlencoded";
        //    message += "&cmd=_notify-validate";
        //    req.ContentLength = message.Length;

        //    //Send the request to PayPal and get the response
        //    using (var streamOut = new StreamWriter(req.GetRequestStream(), System.Text.Encoding.ASCII))
        //    {
        //        streamOut.Write(message);
        //    }

        //    var webResponse = req.GetResponse();
        //    string response;
        //    using (var streamIn = new StreamReader(webResponse.GetResponseStream()))
        //    {
        //        response = streamIn.ReadToEnd();
        //    }

        //    return response;
        //}

        public string RequestPayPal(string urlEncodedContent, string tx)
        {
            string result = null;
            var content = new StringContent(urlEncodedContent, Encoding.ASCII, "application/x-www-form-urlencoded");
            using (var client = new HttpClient())
            {
                var response = client.PostAsync(PayPalAddress, content).Result;
                if (response.IsSuccessStatusCode)
                {
                    result = response.Content.ReadAsStringAsync().Result;
                }
                else
                {
                    var errorMessage = String.Format("StatusCode:{1}; Reason:{2}; RequestContent:{0}", response.StatusCode.ToString(), response.ReasonPhrase, urlEncodedContent);
                    WriteLog(PayPalLogEntity.NotificationKind.Error, tx, errorMessage);
                }
            }
            return result;
        }

        // PDT +https://developer.paypal.com/docs/classic/paypal-payments-standard/integration-guide/paymentdatatransfer/
        public string RequestPaymentDetails(string tx)
        {
            // This is the seller's Payment Data Transfer authorization token found in "Selling Preferences" -> "Website Payment Preferences" under the account.
            var authToken = ConfigurationManager.AppSettings["PayPal.AuthToken"];
            var urlEncodedContent = "cmd=_notify-synch&tx=" + tx + "&at=" + authToken;
            return RequestPayPal(urlEncodedContent, tx);
        }

        public string VerifyIPN(string message, string tx)
        {
            var urlEncodedContent = message + "&cmd=_notify-validate";
            return RequestPayPal(urlEncodedContent, tx);
        }

        public bool PostIncomingPaymentTransaction(IEnumerable<string> lines, string logRowKey)
        {
            //    //check the payment_status is Completed
            //    //check that txn_id has not been previously processed
            //    //check that receiver_email is your Primary PayPal email
            //    //check that payment_amount/payment_currency are correct

            bool result = false;

            var pairs = SplitKeyValuePairs(lines);

            bool post = pairs.ContainsKey("payment_status") ? (pairs["payment_status"] == "Completed") : false;

            if (post)
            {

                post =
                        pairs.ContainsKey("txn_id")
                        && pairs.ContainsKey("option_selection2")
                        && pairs.ContainsKey("receiver_email")
                        && pairs.ContainsKey("mc_currency")
                        && pairs.ContainsKey("mc_gross")
                        && pairs.ContainsKey("mc_fee");

                var extId = pairs["txn_id"];
                var userName = HttpUtility.UrlDecode(pairs["option_selection2"] ?? "");
                decimal amount = 0;
                post = post && decimal.TryParse(pairs["mc_gross"], out amount);
                decimal fee = 0;
                post = post && decimal.TryParse(pairs["mc_fee"], out fee);
                post = post && (pairs["mc_currency"] == "USD");
                post = post && (pairs["receiver_email"] == ReceiverEmail);

                if (!post)
                {
                    throw new ArgumentException("Invalid payment details.");
                }

                // This value is displayd to the sender in the confirmation email from PayPal as "Receipt No".
                string receiptId;
                pairs.TryGetValue("receipt_id", out receiptId);

                var transactionDetails =
                        new XElement("PayPalPayment",
                            new XAttribute("ExtId", extId),
                            new XAttribute("LogRowKey", logRowKey)
                        )
                        .ToString(SaveOptions.DisableFormatting);

                // Do a preliminary check. Otherwise dbo.accPostIncomingPayPalPayment will throw an exception on duplicate payment posting.
                const string sqlCheck = @"
select count(*) from dbo.accPostedPayPalPayments where ExtId = @ExtId;
";
                const string sqlPost = @"
execute dbo.accPostIncomingPayPalPayment @UserName, @Amount, @Fee, @ExtId, @ReceiptId, @Details;
";

                post = (DapperHelper.QueryResiliently<int>(sqlCheck, new { ExtId = extId, }).First()) == 0;

                if (post)
                {
                    DapperHelper.ExecuteResiliently(sqlPost, new
                        {
                            UserName = userName,
                            Amount = amount,
                            Fee = fee,
                            ExtId = extId,
                            ReceiptId = receiptId,
                            Details = transactionDetails
                        });

                    result = true;
                }
            }

            return result;
        }

        public IEnumerable<string> SplitIpnMessage(string message)
        {
            return message.Split('&');
        }

        public IEnumerable<string> SplitPdtMessage(string message)
        {
            var lines = new List<string>();
            StringReader reader = new StringReader(message);
            if (reader.ReadLine() == "SUCCESS")
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    lines.Add(line);
                }
            }
            return lines;
        }

        public IDictionary<string, string> SplitKeyValuePairs(IEnumerable<string> lines)
        {
            var pairs = new Dictionary<string, string>();
            foreach (var item in lines)
            {
                var parts = item.Split('=');
                pairs.Add(parts.FirstOrDefault(), parts.LastOrDefault());
            }
            return pairs;
        }

        /// <summary>
        /// Protect userName from tampering with when sending form data to PayPal.
        /// </summary>
        public static string GetTimestampedUserName(string userName)
        {
            var timeText = DateTime.UtcNow.ToBinary().ToString("X").ToLower(); // 16 chars
            var textToHash = userName + timeText + PayPalUserNameHashSalt;
            var bytesToHash = System.Text.Encoding.UTF8.GetBytes(textToHash);
            var hash = System.Security.Cryptography.MD5.Create().ComputeHash(bytesToHash);
            var hashText = new Guid(hash).ToString("N"); // 32 chars lower-case
            var text = timeText + hashText;
            var chunkSize = 24;
            // 49 chars = 2 chunks * 24 chars + 1 separating space.
            var chunkedText = string.Join(" ", Enumerable.Range(0, text.Length / chunkSize).Select(i => text.Substring(i * chunkSize, chunkSize)));
            return chunkedText;
        }

    }
}