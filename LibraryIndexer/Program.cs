using Dapper;
using Microsoft.Azure.WebJobs;
using Runnymede.Common.LibraryIndex;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LibraryIndexer
{
    // To learn more about Microsoft Azure WebJobs, please see http://go.microsoft.com/fwlink/?LinkID=401557
    class Program
    {

        static void Main()
        {
            Console.WriteLine("Starting...");

            // Set the AzureWebJobsDashboard connectionString
            // +http://azure.microsoft.com/blog/2014/09/22/announcing-the-1-0-0-rc1-of-microsoft-azure-webjobs-sdk/
            //JobHost host = new JobHost();
            //host.Call(typeof(Program).GetMethod("Process"));

            Process1();

            Console.Write("Complete. Press <enter> to continue: ");
            Console.ReadLine();
        }

        [NoAutomaticTrigger]
        private static void Process1()
        {
            InternalProcess().Wait();
        }

        private static async Task InternalProcess()
        {
            try
            {
                var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

                var recreate = true;
                var reindexAllResources = true;

                var indexHelper = new CommonIndexHelper(connectionString);
                //var indexHelper = new PersonalIndexHelper(connectionString);

                using (indexHelper)
                {
                    if (recreate)
                    {
                        indexHelper.RecreateIndex();
                    }
                    await indexHelper.IndexResources(recreate || reindexAllResources);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Unhandled exception caught:");
                while (e != null)
                {
                    Console.WriteLine("\t{0}", e.Message);
                    e = e.InnerException;
                }
            }
        }

    }
}
