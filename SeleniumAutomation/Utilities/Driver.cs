using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Runnymede.SeleniumAutomation.Utilities
{
    public class Driver
    {
        static TimeSpan ImplicitWait = TimeSpan.FromSeconds(5);

        public static IWebDriver Instance { get; set; } // Singleton
        public static string BaseHttpsAddress
        {
            get
            {
                return "https://deva.englisharium.com/";
            }
        }

        public static void Initialize()
        {
            if (Instance == null)
            {
                Instance = new ChromeDriver();
                TurnOnWait();
            }
        }

        public static void Close()
        {
            if (Instance != null)
            {
                var instance = Instance;
                Instance = null;
                //instance.Close(); // This command does not close the chromedriver.exe
                instance.Quit();
            }
        }

        public static void Wait(int seconds)
        {
            Thread.Sleep(seconds * 1000);
        }

        public static void NoWait(Action action)
        {
            TurnOffWait();
            action();
            TurnOnWait();
        }

        public static void TurnOffWait()
        {
            Instance.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(0));
        }
        public static void TurnOnWait()
        {
            Instance.Manage().Timeouts().ImplicitlyWait(ImplicitWait);
        }


    }
}
