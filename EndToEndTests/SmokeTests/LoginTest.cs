using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Runnymede.SeleniumAutomation;
using Runnymede.EndToEndTests.Utilities;
using Runnymede.SeleniumAutomation.Pages;
using Runnymede.SeleniumAutomation.Utilities;

namespace Runnymede.EndToEndTests.SmokeTests
{
    [TestClass]
    public class LoginTest : CustomTest
    {

        [TestMethod]
        public void learner_user_can_login()
        {
            LoginAsLearner();
            Assert.IsTrue(HomePage.IsAt, "Failed to login");
            Assert.IsTrue(CustomPage.IsLoggedIn, "Failed to login");
        }
    }
}
