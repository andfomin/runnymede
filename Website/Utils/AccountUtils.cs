using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Xml.Linq;
using System.Net.Http;
using System.Text;
using System.Security.Principal;
using Microsoft.AspNet.Identity;
using System.Security.Claims;
using Runnymede.Website.Models;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Runnymede.Website.Utils
{
    public static class AccountUtils
    {
        public const string KeeperCookieName = "keeper";
        private const string CodeKey = "code";
        private const string UserIdKey = "time"; // Originally "userId". Security through obscurity :(
        private const string skip32Key = "yh5bvgCWew"; // Key must be 10 bytes long.
        public const int AvatarLargeSize = 120;
        public const int AvatarSmallSize = 36;

        #region Identity utils

        public static int GetUserId(IIdentity identity)
        {
            return Convert.ToInt32(identity.GetUserId()); // Extention method in Microsoft.AspNet.Identity
        }

        public static string GetUserName(IIdentity identity)
        {
            return identity.GetUserName();
        }

        public static string GetUserDisplayName(IIdentity identity)
        {
            var claimsIdentity = identity as System.Security.Claims.ClaimsIdentity;
            return claimsIdentity != null ? claimsIdentity.FindFirstValue(AppClaimTypes.DisplayName) : null; // FindFirstValue() returns null if not found.
        }

        public static bool GetUserIsTeacher(IIdentity identity)
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

        public static int GetUserId(this Controller controller)
        {
            return GetUserId(controller.User.Identity);
        }

        public static string GetUserDisplayName(this Controller controller)
        {
            return GetUserDisplayName(controller.User.Identity);
        }

        public static bool GetUserIsTeacher(this Controller controller)
        {
            return GetUserIsTeacher(controller.User.Identity);
        }

        #endregion

        #region ApiController extension methods

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
            return GetUserDisplayName(controller.RequestContext.Principal.Identity);
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
            var cipher = new Eleven41.Skip32.Skip32Cipher(Encoding.ASCII.GetBytes(skip32Key));
            var encrypted = cipher.Encrypt(userId);
            // Convert to UInt32 to avoid a sign.
            var bytes = BitConverter.GetBytes(encrypted);
            var unsigned = BitConverter.ToUInt32(bytes, 0);
            var encryptedString = unsigned.ToString();
            return CodeKey + "=" + HttpUtility.UrlEncode(code) + "&" + UserIdKey + "=" + HttpUtility.UrlEncode(encryptedString);
        }

        public static int GetUserIdFromQueryStringValue(string queryStringValue)
        {
            uint unsigned;
            if (UInt32.TryParse(queryStringValue, out unsigned))
            {
                var bytes = BitConverter.GetBytes(unsigned);
                var encrypted = BitConverter.ToInt32(bytes, 0);
                var cipher = new Eleven41.Skip32.Skip32Cipher(Encoding.ASCII.GetBytes(skip32Key));
                return cipher.Decrypt(encrypted);
            }
            else
                return 0;
        }


        #endregion

        #region Keeper cookie utils

        public static async Task EnsureKeeperCookieAsync(this Controller controller)
        {
            var request = controller.Request;

            if (request.Browser.Crawler)
                return;

            if (!request.Cookies.AllKeys.Contains(KeeperCookieName))
            {
                // Set the Keeper cookie
                var value = Guid.NewGuid().ToString("N").ToUpper();
                var cookie = new HttpCookie(KeeperCookieName, value);
                cookie.Expires = DateTime.UtcNow.AddYears(1);
                controller.Response.Cookies.Add(cookie);

                // Log the contact.
                var referer = request.UrlReferrer != null ? request.UrlReferrer.AbsoluteUri : null;
                var languages = request.UserLanguages ?? Enumerable.Empty<string>();

                var logData =
                    new XElement("LogData",
                        new XElement("Kind", LoggingUtils.Kind.Referer),
                        new XElement("Time", DateTime.UtcNow),
                        new XElement("Keeper", value),
                        new XElement("RefererUrl", referer),
                        new XElement("Host", request.UserHostAddress),
                        new XElement("UserAgent", request.UserAgent),
                        new XElement("UserLanguages",
                            from language in languages
                            select new XElement("Language", language)
                        )
                    )
                    .ToString(SaveOptions.DisableFormatting);

                await LoggingUtils.WriteKeeperLogAsync(logData);
            }
        }

        public static Guid GetKeeper(this Controller controller)
        {
            var requestCookies = controller.Request.Cookies;
            var responseCookies = controller.Response.Cookies;
            var cookies = requestCookies.AllKeys.Contains(KeeperCookieName)
                ? requestCookies
                : (
                    responseCookies.AllKeys.Contains(KeeperCookieName)
                    ? responseCookies
                    : null
                );
            //Not Guid.TryParseExact(keeperCookieValue, "N", out keeper); If input is null, it does not throw an exception, but outputs Guid.Empty.
            return cookies != null ? new Guid(cookies[KeeperCookieName].Value) : Guid.Empty;
        }

        public static Guid GetKeeper(this ApiController controller)
        {
            var cookies = controller.Request.Headers.GetCookies(KeeperCookieName).FirstOrDefault();
            var cookie = cookies != null ? cookies.Cookies.FirstOrDefault() : null;
            return cookie != null ? new Guid(cookie.Value) : Guid.Empty;
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
        /// Creates a square identicon.
        /// </summary>
        /// <param name="seed">Seed</param>
        /// <param name="stream">Output</param>
        /// <param name="blockSize">The size of blocks in pixels. Total 6 blocks.</param>
        private static void CreateIdenticon(Guid seed, Stream stream, int blockSize) // Total 6 blocks. 20 x 6 = 120; 8 x 6 = 48
        {
            int size = 5; // blocks
            var bitmapSize = (size + 1) * blockSize; // pixels
            var offset = blockSize / 2; // pixels

            byte grayValue = 0xf0;
            Color bgColor = Color.FromArgb(255, grayValue, grayValue, grayValue);

            int hue = Math.Abs(seed.GetHashCode()) % 360;
            var color = ColorFromAhsb(255, hue, 1, 0.4f);

            var byteArr = seed.ToByteArray();
            var arrLength = byteArr.Length;

            using (var bitmap = new System.Drawing.Bitmap(bitmapSize, bitmapSize))
            using (var graphics = Graphics.FromImage(bitmap))
            {
                using (var bgBrush = new SolidBrush(bgColor))
                {
                    graphics.FillRectangle(bgBrush, 0, 0, bitmapSize, bitmapSize);
                }

                using (var brush = new SolidBrush(color))
                {
                    for (var x = 0; x < size / 2 + 1; x++)
                    {
                        for (var y = 0; y < size; y++)
                        {
                            var i = x * size + y;
                            var fill = byteArr[i % 16] < 128;

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
                                Rectangle rect2 = new Rectangle(offset + (size - x - 1) * blockSize, offset + y * blockSize, blockSize, blockSize);
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
        /// A fire-and-forget method because it is async void.
        /// </summary>
        /// <param name="extId">Seed</param>
        /// <param name="blockSize">The size of blocks in pixels. Total 6 blocks.</param>
        /// <param name="containerName"></param>
        public static async Task CreateAndSaveIdenticonAsync(Guid extId, int blockSize, string containerName)
        {
            var blobName = extId.ToString().ToUpper();

            using (var stream = new MemoryStream())
            {
                CreateIdenticon(extId, stream, blockSize);
                await AzureStorageUtils.UploadBlobAsync(stream, containerName, blobName, "image/png");
            }
        }

        #endregion



    }
}