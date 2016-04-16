using Microsoft.VisualStudio.TestTools.UnitTesting;
using Runnymede.SeleniumAutomation.Pages;
using Runnymede.SeleniumAutomation.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Runnymede.EndToEndTests.Utilities
{
    public class CustomTest
    {
        [TestInitialize]
        public void Init()
        {
            Driver.Initialize();
        }

        [TestCleanup]
        public void Cleanup()
        {
            Driver.Close();
        }

        public void LoginAsLearner()
        {
            LoginPage.GoTo();
            LoginPage.LoginAs("q@q.q").WithPassword("123456").Login();
        }

    }
}
