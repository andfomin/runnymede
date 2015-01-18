using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.WebPages;
using Microsoft.AspNet.Identity;
using Runnymede.Website.Utils;
using Newtonsoft.Json;

namespace Runnymede.Website.Models
{
    public class PageViewModel
    {
        public string UserName { get; set; }
        public int UserId { get; set; }
        public string DisplayName { get; set; }
        public bool IsTeacher { get; set; }

        public PageViewModel(WebPageRenderingBase page)
        {
            if (page.Request.IsAuthenticated)
            {
                var identity = page.User.Identity as System.Security.Claims.ClaimsIdentity;
                if (identity != null)
                {
                    if (page.Request.IsSecureConnection)
                    {
                        UserName = identity.GetUserName();
                    }
                    UserId = Convert.ToInt32(identity.GetUserId());
                    DisplayName = identity.FindFirstValue(AppClaimTypes.DisplayName) ?? UserName;
                    IsTeacher = identity.HasClaim(i => i.Type == AppClaimTypes.IsTeacher);
                }
            }
        }

        public object SelfUserParam
        {
            get
            {
                return new
                {
                    UserName = UserName,
                    Id = UserId,
                    DisplayName = DisplayName,
                    IsTeacher = IsTeacher,
                };
            }
        }

    }
}