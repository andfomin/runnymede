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
            ConfigureSignalR(app);
        }

        private void ConfigureSignalR(IAppBuilder app)
        {
            // Scale out with Redis backplane
            var redisServer = ConfigurationManager.AppSettings["RedisServer"];
            var redisPort = ConfigurationManager.AppSettings["RedisPort"];
            var redisPassword = String.Empty; // ConfigurationManager.AppSettings["RedisKey"];
            GlobalHost.DependencyResolver.UseRedis(redisServer, Int32.Parse(redisPort), redisPassword, "englm");

            GlobalHost.Configuration.DefaultMessageBufferSize = 100; // Each signal has a buffer of messages in RAM. A signal is a broadcast direction, i.e. HubName plus a receiver (All, Group, User)
            
            app.MapSignalR();
        }
    }
}
