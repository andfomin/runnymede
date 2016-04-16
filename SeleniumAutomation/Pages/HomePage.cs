using OpenQA.Selenium;
using Runnymede.SeleniumAutomation.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Runnymede.SeleniumAutomation.Pages
{
    public class HomePage
    {

        public static bool IsAt
        {
            get
            {
                var elems = Driver.Instance.FindElements(By.Id("myJumbotron"));
                return elems.Any();
            }
        }

    }
}
