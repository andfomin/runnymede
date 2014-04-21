using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Runnymede.Website.Models;
using Microsoft.Owin.Security;
using System.Text;
using System.Security.Principal;
using Microsoft.AspNet.Identity;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity.Owin;
using System.Security.Claims;

namespace Runnymede.Website.Utils
{
    public static class IdentityHelper
    {
        private const string CodeKey = "code";
        public static string GetCodeFromRequest(HttpRequestBase request)
        {
            return request.QueryString[CodeKey];
        }

        private const string UserIdKey = "time"; // Originally "userId". Security through obscurity :(
        private const string skip32Key = "yh5bvgCWew"; // Key must be 10 bytes long.

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

        private static bool IsLocalUrl(string url)
        {
            return !string.IsNullOrEmpty(url) && ((url[0] == '/' && (url.Length == 1 || (url[1] != '/' && url[1] != '\\'))) || (url.Length > 1 && url[0] == '~' && url[1] == '/'));
        }

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
    }


}