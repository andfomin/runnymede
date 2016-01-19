using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Runnymede.SeleniumAutomation.Utilities
{
    public class CustomPage
    {
        public static bool IsLoggedIn
        {
            get
            {
                const string scr = "app.selfUserParam =";
                var html = Driver.Instance.PageSource;
                return html.IndexOf(scr) > 0;
            }
        }

    }
}
