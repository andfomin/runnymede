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

namespace Runnymede.Website.Utils
{
    public class PayPalHelper
    {
        private const string PayPalAddress = "https://www.paypal.com/cgi-bin/webscr";

        public PayPalHelper()
        {
        }

        public string WriteLog(PayPalLogEntity.NotificationKind kind, string tx, string logData)
        {
            if (string.IsNullOrEmpty(tx))
            {
                throw new ArgumentException();
            }

            var rowKey = LoggingUtils.GetUniquifiedObservedTime();

            var entity = new PayPalLogEntity
            {
                PartitionKey = tx,
                RowKey = rowKey,
                Kind = kind.ToString(),
                LogData = logData,
            };
            AzureStorageUtils.InsertEntry(AzureStorageUtils.TableNames.PaymentLog, entity);

            return rowKey;
        }

        public string RequestPaymentDetails(string tx)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(PayPalAddress);

            //Set values for the request
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";

            // This is the seller's Payment Data Transfer authorization token found in "Website Payment Preferences" under the account.
            string authToken = "RaKmpVPpjxMbCfvh21wxGlDohA8hLIFJov7rSoWZLQ6ib7xcWKDs4Dvs3N8";
            string query = "cmd=_notify-synch&tx=" + tx + "&at=" + authToken;
            req.ContentLength = query.Length;
            using (var streamOut = new StreamWriter(req.GetRequestStream(), System.Text.Encoding.ASCII))
            {
                streamOut.Write(query);
            }

            //Send the request to PayPal and get the response
            var webResponse = req.GetResponse();
            string response;
            using (var streamIn = new StreamReader(webResponse.GetResponseStream()))
            {
                response = streamIn.ReadToEnd();
            }

            return response;
        }


        public string VerifyIPN(string message)
        {
            //Post back
            var req = (HttpWebRequest)WebRequest.Create(PayPalAddress);
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            message += "&cmd=_notify-validate";
            req.ContentLength = message.Length;

            //Send the request to PayPal and get the response
            using (var streamOut = new StreamWriter(req.GetRequestStream(), System.Text.Encoding.ASCII))
            {
                streamOut.Write(message);
            }

            var webResponse = req.GetResponse();
            string response;
            using (var streamIn = new StreamReader(webResponse.GetResponseStream()))
            {
                response = streamIn.ReadToEnd();
            }

            return response;
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
                var userName = HttpUtility.UrlDecode(pairs["option_selection2"]);
                decimal amount = 0;
                post = post && decimal.TryParse(pairs["mc_gross"], out amount);
                decimal fee = 0;
                post = post && decimal.TryParse(pairs["mc_fee"], out fee);
                post = post && (pairs["mc_currency"] == "USD");
                post = post && (pairs["receiver_email"] == "paypal%40englc.com");

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




    }
}