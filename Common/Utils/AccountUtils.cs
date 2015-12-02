using Microsoft.AspNet.Identity;
using Runnymede.Common.Utils;
using Runnymede.Common.Models;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Xml.Linq;
using System.Web.Hosting;

namespace Runnymede.Common.Utils
{
    public static class AccountUtils
    {
        public const string ExtIdCookieName = "extid";
        private const string CodeKey = "code";
        private const string UserIdKey = "timestamp"; // Originally "userId". Security through obscurity :(
        private const string skip32Key = "yh5bvgCWew"; // Key must be 10 chars long.
        // Each icon consists of 8 x 8 blocks (5 blocks pattern + 2 x 1.5 margin). Notice, the block size should be an even number to produce equal margins.
        public const int AvatarLargeSize = 96; // 8 * 12 = 96
        public const int AvatarSmallSize = 32; // 8 * 4 = 32

        //public const string NotAuthenticatedErrorMessage = "Please log in."; // With ApiController methods use Runnymede.Website.Utils.AppPoliteAuthorizeAttribute instead.

        #region Identity utils

        // We might allow anonymous users to use features. And we may respect the authorized users' identity.
        /// Use ASP.NET Identity to see if the user is logged in. If they are, we can then get their UserId.
        public static bool IsAuthenticated(IIdentity identity)
        {
            return identity.IsAuthenticated;
        }

        public static int GetUserId(IIdentity identity)
        {
            var userIdStr = identity.GetUserId(); // Identity.GetUserId() is an extention method in Microsoft.AspNet.Identity
            int result;
            Int32.TryParse(userIdStr, out result); // Assigns zero if the conversion failed.
            return result;
        }

        private static string GetUserName(IIdentity identity)
        {
            return identity.GetUserName();
        }

        private static string GetStringClaim(IIdentity identity, string claimType)
        {
            var claimsIdentity = identity as System.Security.Claims.ClaimsIdentity;
            //return (claimsIdentity != null ? claimsIdentity.FindFirstValue(claimType) : null) ?? String.Empty; // FindFirstValue() returns null if not found.
            return claimsIdentity != null ? claimsIdentity.FindFirstValue(claimType) : null;
        }

        private static bool GetUserIsTeacher(IIdentity identity)
        {
            var claimsIdentity = identity as System.Security.Claims.ClaimsIdentity;
            return (claimsIdentity != null) && claimsIdentity.HasClaim(i => i.Type == AppClaimTypes.IsTeacher);
        }

        public static string PlainErrorMessage(this IdentityResult result, string defaultErrorMessage = null)
        {
            return result.Succeeded
                    ? null
                    : result.Errors != null ? string.Join("\n", result.Errors) : defaultErrorMessage;
        }

        #endregion

        #region Controller extension methods

        public static bool IsAuthenticated(this Controller controller)
        {
            return IsAuthenticated(controller.User.Identity);
        }

        public static int GetUserId(this Controller controller)
        {
            return GetUserId(controller.User.Identity);
        }

        public static string GetUserName(this Controller controller)
        {
            return GetUserName(controller.User.Identity);
        }

        public static string GetUserDisplayName(this Controller controller)
        {
            return GetStringClaim(controller.User.Identity, AppClaimTypes.DisplayName);
        }

        public static bool GetUserIsTeacher(this Controller controller)
        {
            return GetUserIsTeacher(controller.User.Identity);
        }

        #endregion

        #region ApiController extension methods

        public static bool IsAuthenticated(this ApiController controller)
        {
            return IsAuthenticated(controller.RequestContext.Principal.Identity);
        }

        public static int GetUserId(this ApiController controller)
        {
            return GetUserId(controller.RequestContext.Principal.Identity);
        }

        public static string GetUserName(this ApiController controller)
        {
            return GetUserName(controller.RequestContext.Principal.Identity);
        }

        public static string GetUserDisplayName(this ApiController controller)
        {
            return GetStringClaim(controller.RequestContext.Principal.Identity, AppClaimTypes.DisplayName);
        }

        public static bool GetUserIsTeacher(this ApiController controller)
        {
            return GetUserIsTeacher(controller.RequestContext.Principal.Identity);
        }

        #endregion

        #region Confirmation mail utils

        public static string GetCodeFromRequest(HttpRequestBase request)
        {
            return request.QueryString[CodeKey];
        }

        public static int GetUserIdFromRequest(HttpRequestBase request)
        {
            var queryStringValue = HttpUtility.UrlDecode(request.QueryString[UserIdKey]);
            return GetUserIdFromQueryStringValue(queryStringValue);
        }

        public static string GetMailLinkQueryString(string code, int userId)
        {
            // 32-bit block cipher. +https://github.com/eleven41/Eleven41.Skip32
            //var cipher = new Eleven41.Skip32.Skip32Cipher(Encoding.ASCII.GetBytes(skip32Key));
            //var encrypted = cipher.Encrypt(userId);
            //// Convert to UInt32 to avoid a sign.
            //var bytes = BitConverter.GetBytes(encrypted);
            //var unsigned = BitConverter.ToUInt32(bytes, 0);
            //var encryptedString = unsigned.ToString();
            var encryptedString = Skip32Utils.EncriptIntToHexString(userId, skip32Key);
            return CodeKey + "=" + HttpUtility.UrlEncode(code) + "&" + UserIdKey + "=" + HttpUtility.UrlEncode(encryptedString);
        }

        public static int GetUserIdFromQueryStringValue(string queryStringValue)
        {
            //uint unsigned;
            //if (UInt32.TryParse(queryStringValue, out unsigned))
            //{
            //    var bytes = BitConverter.GetBytes(unsigned);
            //    var encrypted = BitConverter.ToInt32(bytes, 0);
            //    var cipher = new Eleven41.Skip32.Skip32Cipher(Encoding.ASCII.GetBytes(skip32Key));
            //    return cipher.Decrypt(encrypted);
            //}
            //else
            //    return 0;
            return Skip32Utils.DecriptHexStringToInt(queryStringValue, skip32Key, 0);
        }

        #endregion

        #region Keeper cookie utils

        public static void EnsureExtIdCookie(this Controller controller)
        {
            var request = controller.Request;

            if (request.Browser.Crawler)
                return;

            if (!request.Cookies.AllKeys.Contains(ExtIdCookieName))
            {
                // Set the ExtId cookie
                var value = KeyUtils.GetTwelveBase32Digits();
                var cookie = new HttpCookie(ExtIdCookieName, value);
                cookie.Expires = DateTime.UtcNow.AddYears(1);
                controller.Response.Cookies.Add(cookie);

                // Log the contact.
                var referer = request.UrlReferrer != null ? request.UrlReferrer.AbsoluteUri : null;
                var languages = request.UserLanguages ?? Enumerable.Empty<string>();

                //var logData0 =
                //    new XElement("LogData",
                //        new XElement("Kind", LoggingUtils.Kind.Referer),
                //        new XElement("Time", DateTime.UtcNow),
                //        new XElement("ExtId", value),
                //        new XElement("RefererUrl", referer),
                //        new XElement("Host", request.UserHostAddress),
                //        new XElement("UserAgent", request.UserAgent),
                //        new XElement("UserLanguages",
                //            from language in languages
                //            select new XElement("Language", language)
                //        )
                //    )
                //    .ToString(SaveOptions.DisableFormatting);

                var logData = JsonUtils.SerializeAsJson(
                    new KeeperLogData
                    {
                        Kind = LoggingUtils.Kind.Referer,
                        Time = DateTime.UtcNow,
                        ExtId = value,
                        RefererUrl = referer,
                        Host = request.UserHostAddress,
                        UserAgent = request.UserAgent,
                        Languages = languages,
                    });

                //await LoggingUtils.WriteKeeperLogAsync(logData);
                HostingEnvironment.QueueBackgroundWorkItem(async ct => await LoggingUtils.WriteKeeperLogAsync(logData));
            }
        }

        public static string GetExtId(this Controller controller)
        {
            var requestCookies = controller.Request.Cookies;
            var responseCookies = controller.Response.Cookies;
            var cookies = requestCookies.AllKeys.Contains(ExtIdCookieName)
                ? requestCookies
                : (
                    responseCookies.AllKeys.Contains(ExtIdCookieName)
                    ? responseCookies
                    : null
                );
            return cookies != null ? cookies[ExtIdCookieName].Value : null;
        }

        public static string GetExtId(this ApiController controller)
        {
            var cookies = controller.Request.Headers.GetCookies(ExtIdCookieName).FirstOrDefault();
            return cookies != null ? cookies[ExtIdCookieName].Value : null;
        }

        #endregion

        #region Identicon utils

        // +http://labs.astrobunny.net/blog/2011/11/22/making-color-from-alpha-hue-saturation-and-brightness/
        private static Color ColorFromAhsb(int a, float h, float s, float b)
        {

            if (0 > a || 255 < a)
            {
                throw new ArgumentOutOfRangeException("a");
            }
            if (0f > h || 360f < h)
            {
                throw new ArgumentOutOfRangeException("h");
            }
            if (0f > s || 1f < s)
            {
                throw new ArgumentOutOfRangeException("s");
            }
            if (0f > b || 1f < b)
            {
                throw new ArgumentOutOfRangeException("b");
            }

            if (0 == s)
            {
                return Color.FromArgb(a, Convert.ToInt32(b * 255),
                    Convert.ToInt32(b * 255), Convert.ToInt32(b * 255));
            }

            float fMax, fMid, fMin;
            int iSextant, iMax, iMid, iMin;

            if (0.5 < b)
            {
                fMax = b - (b * s) + s;
                fMin = b + (b * s) - s;
            }
            else
            {
                fMax = b + (b * s);
                fMin = b - (b * s);
            }

            iSextant = (int)Math.Floor(h / 60f);
            if (300f <= h)
            {
                h -= 360f;
            }
            h /= 60f;
            h -= 2f * (float)Math.Floor(((iSextant + 1f) % 6f) / 2f);
            if (0 == iSextant % 2)
            {
                fMid = h * (fMax - fMin) + fMin;
            }
            else
            {
                fMid = fMin - h * (fMax - fMin);
            }

            iMax = Convert.ToInt32(fMax * 255);
            iMid = Convert.ToInt32(fMid * 255);
            iMin = Convert.ToInt32(fMin * 255);

            switch (iSextant)
            {
                case 1:
                    return Color.FromArgb(a, iMid, iMax, iMin);
                case 2:
                    return Color.FromArgb(a, iMin, iMax, iMid);
                case 3:
                    return Color.FromArgb(a, iMin, iMid, iMax);
                case 4:
                    return Color.FromArgb(a, iMid, iMin, iMax);
                case 5:
                    return Color.FromArgb(a, iMax, iMin, iMid);
                default:
                    return Color.FromArgb(a, iMax, iMid, iMin);
            }
        } // end of ColorFromAhsb()

        /// <summary>
        /// Creates a square identicon as PNG.
        /// </summary>
        /// <param name="userId">We expect a random value from 1073741823 to 2147483646</param>
        /// <param name="stream">Output</param>
        /// <param name="size">The size of the icon in pixels. Total 5 blocks pattern + 2 x 1.5 margin = 8 blocks of pixels. Notice, the block size should be an even number to produce equal margins.</param>
        private static void CreateIdenticon(int userId, Stream stream, int size)
        {
            int avatarSize = 5; // blocks
            var blockSize = size / (avatarSize + 3); // pixels
            var offset = blockSize * 3 / 2; // pixels

            // We need 5x3=15 random values. Seed/ExtId length is 12 chars.
            // seed's distribution is not random. Its generation is based on time. Eight first characters are sequesial.
            // Plain-vanila String.GetHashCode() has not-well-enough uniform distribution. We use MD5, but I do not know how good its distribution is for such a short string :(.
            //var hashedSeed = MD5.Create().ComputeHash(Encoding.ASCII.GetBytes(seed));
            // We use Guid as an intermediate data type to convert the byte array from MD5 which is 16 bytes long, to integer.
            //var random = new Random(new Guid(hashedSeed).GetHashCode());

            // We expect userId is a random value from 1073741823 to 2147483646.
            var random = new Random(userId);

            byte grayValue = 0xf0;
            Color bgColor = Color.FromArgb(255, grayValue, grayValue, grayValue);

            var hue = random.Next(360);
            var color = ColorFromAhsb(255, hue, 1, 0.4f);

            using (var bitmap = new System.Drawing.Bitmap(size, size))
            using (var graphics = Graphics.FromImage(bitmap))
            {
                using (var bgBrush = new SolidBrush(bgColor))
                {
                    graphics.FillRectangle(bgBrush, 0, 0, size, size);
                }

                using (var brush = new SolidBrush(color))
                {
                    for (var x = 0; x < avatarSize / 2 + 1; x++)
                    {
                        for (var y = 0; y < avatarSize; y++)
                        {
                            var i = x * avatarSize + y;
                            //var fill = byteArr[i % arrLength] < 128;
                            var fill = random.Next(2) == 0;

                            // We may "humanize" the icon look by sacrificing collisions durability.
                            //if (false)
                            //{
                            //    if ((x == 0) && (y == 0))
                            //        fill = false; // shoulders
                            //    if ((x == size / 2) && (y == 0))
                            //        fill = true; // head
                            //    if ((x == size / 2) && (y == size / 2))
                            //        fill = true; // body
                            //    if ((x == size / 2) && (y == size - 1))
                            //        fill = false; // split feet
                            //}

                            if (fill)
                            {
                                Rectangle rect1 = new Rectangle(offset + x * blockSize, offset + y * blockSize, blockSize, blockSize);
                                graphics.FillRectangle(brush, rect1);
                                // make it horizontally symmetrical
                                Rectangle rect2 = new Rectangle(offset + (avatarSize - x - 1) * blockSize, offset + y * blockSize, blockSize, blockSize);
                                graphics.FillRectangle(brush, rect2);
                            }
                        }
                    }
                }

                if (stream != null)
                {
                    bitmap.Save(stream, ImageFormat.Png);
                }
            }
        }

        /// <summary>
        /// Creates the PNG identicon image corresponding to userId and saves it to a blob
        /// </summary>
        /// <param name="userId">We expect a random value from 1073741823 to 2147483646</param>
        /// <param name="size">The size of icon in pixels. Total 5 blocks pattern + 2 x 1.5 margin = 8 blocks of pixels. Notice, the block size should be an even number to produce equal margins.</param>
        /// <param name="containerName"></param>
        public static async Task CreateAndSaveIdenticonAsync(int userId, int size, string containerName)
        {
            using (var stream = new MemoryStream())
            {
                CreateIdenticon(userId, stream, size);
                var blobName = KeyUtils.IntToKey(userId);
                await AzureStorageUtils.UploadBlobAsync(stream, containerName, blobName, "image/png");
            }
        }

        #endregion



    }
}