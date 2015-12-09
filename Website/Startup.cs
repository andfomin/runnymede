using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Owin;
using Owin;
using Microsoft.AspNet.SignalR;
using System.Configuration;

[assembly: OwinStartup(typeof(Runnymede.Website.Startup))]

namespace Runnymede.Website
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            ConfigureSignalR(app); // See +https://github.com/SignalR/SignalR/issues/3548
        }

        private void ConfigureSignalR(IAppBuilder app)
        {
            // Scale out SignalR using Azure Service Bus backplane.
#if DEBUG
            //var topicPrefix = "test01"; // Use for development
#else
            var topicPrefix = "englm"; 
            var connectionString = ConfigurationManager.AppSettings["Azure.ServiceBus.ConnectionString"];
            GlobalHost.DependencyResolver.UseServiceBus(connectionString, topicPrefix);
#endif

            GlobalHost.Configuration.DefaultMessageBufferSize = 100; // Each signal has a buffer of messages in RAM. A signal is a broadcast direction, i.e. HubName plus a receiver (All, Group, User)           
            app.MapSignalR();
        }
    }
}
