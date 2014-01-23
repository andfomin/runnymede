using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.WebPages;
using Microsoft.AspNet.Identity;
using Runnymede.Website.Utils;

namespace Runnymede.Website.Models
{
    public class PageViewModel
    {
        public string UserName { get; set; }
        public string DisplayName { get; set; }
        public bool IsTutor { get; set; }
        public int UserId { get; set; }

        public PageViewModel(WebPageRenderingBase page)
        {
            if (page.Request.IsAuthenticated)
            {
                var identity = page.User.Identity as System.Security.Claims.ClaimsIdentity;
                if (identity != null)
                {
                    UserId = Convert.ToInt32(identity.GetUserId());
                    UserName = identity.GetUserName();
                    DisplayName = identity.FindFirstValue(AppClaimTypes.DisplayName) ?? UserName;
                    IsTutor = identity.HasClaim(i => i.Type == AppClaimTypes.IsTutor);
                }
            }
        }
    }
}