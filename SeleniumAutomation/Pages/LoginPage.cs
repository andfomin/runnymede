using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Runnymede.SeleniumAutomation.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Runnymede.SeleniumAutomation.Pages
{
    public class LoginPage
    {
        public static void GoTo()
        {
            Driver.Instance.Navigate().GoToUrl(Driver.BaseHttpsAddress + "account/login");
            var wait = new WebDriverWait(Driver.Instance, TimeSpan.FromSeconds(7));
            wait.Until(i => i.SwitchTo().ActiveElement().GetAttribute("id") == "userName");
        }

        public static LoginCommand LoginAs(string userName)
        {
            return new LoginCommand(userName);
        }

        public class LoginCommand
        {
            private string password;
            private readonly string userName;

            public LoginCommand(string userName)
            {
                this.userName = userName;
            }

            public LoginCommand WithPassword(string password)
            {
                this.password = password;
                return this;
            }
            public void Login()
            {
                var loginInput = Driver.Instance.FindElement(By.Id("userName"));
                loginInput.SendKeys(userName);
                var passwordInput = Driver.Instance.FindElement(By.Id("password"));
                passwordInput.SendKeys(password);
                var loginButton = Driver.Instance.FindElement(By.ClassName("app-btn-disablable"));
                loginButton.Click();
            }
        }
    }
}
