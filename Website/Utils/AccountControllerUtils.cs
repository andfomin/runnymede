using System;
using System.Linq;

namespace Runnymede.Website.Utils
{
    public static class AccountControllerUtils
    {
        public const string PayPalUserNameHashSalt = "#Salt7B190B1B03B9#";

        /// <summary>
        /// Protect userName from tampering with when sending form data to PayPal.
        /// </summary>
        public static string GetTimestampedUserName(string userName)
        {
            var timeText = DateTime.UtcNow.ToBinary().ToString("X"); // 16 chars
            var textToHash = userName + timeText + PayPalUserNameHashSalt;
            var bytesToHash = System.Text.Encoding.UTF8.GetBytes(textToHash);
            var hash = System.Security.Cryptography.MD5.Create().ComputeHash(bytesToHash);
            var hashText = new Guid(hash).ToString("N").ToUpper(); // 32 chars
            var text = timeText + hashText;
            var chunkSize = 24;
            // 49 chars = 2 chunks * 24 chars + 1 separating space.
            var chunkedText = string.Join(" ", Enumerable.Range(0, text.Length / chunkSize).Select(i => text.Substring(i * chunkSize, chunkSize)));
            return chunkedText;
        }

        public static int? CalculateTimeOffsetSec(string localTime)
        {
            if (!string.IsNullOrWhiteSpace(localTime))
            {
                // We get the localTime value from the wild web. Be cautious.
                try
                {
                    var parts = localTime.Split('/');
                    int dummy;
                    if (parts.Count() == 6 && parts.All(i => Int32.TryParse(i, out dummy)))
                    {
                        var arr = parts.Select(i => Convert.ToInt32(i)).ToArray();
                        var clientTime = new DateTime(arr[0], arr[1], arr[2], arr[3], arr[4], arr[5], DateTimeKind.Utc);
                        var span = clientTime - DateTime.UtcNow;
                        var timeOffsetSec = Convert.ToInt32(span.TotalSeconds);
                        return timeOffsetSec;
                    }
                }
                catch
                {
                }
            }

            return null;
        }



    }
}