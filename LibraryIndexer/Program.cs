using Runnymede.Common.LibraryIndex;
using Runnymede.Common.Utils;
using System;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;
using System.Linq;

namespace LibraryIndexer
{
    class Program
    {

        static void Main()
        {
            //Console.WriteLine("Starting...");

            // Set the AzureWebJobsDashboard connectionString
            // +http://azure.microsoft.com/blog/2014/09/22/announcing-the-1-0-0-rc1-of-microsoft-azure-webjobs-sdk/
            //JobHost host = new JobHost();
            //host.Call(typeof(Program).GetMethod("Process"));

            //DoProcessing().Wait();

            //Console.Write("Complete. Press <enter> to continue: ");
            //Console.ReadLine();

            DoItalki();
        }

        private static void DoItalki()
        {
            //var helper = new ItalkiHelper();
            //// Console.WriteLine("press any key...");
            ////Console.ReadKey();
            ////var html = helper.LoadPage(ItalkiHelper.Url32892).Result;
            //var html = File.ReadAllText(@"C:\Users\Andrey\Desktop\ex01.html");
            //var vacantSlots = helper.ProcessPage(html);

            //foreach (var slot in vacantSlots.Where(i => i.Duration.TotalMinutes >= ItalkiHelper.SessionDuration))
            ////foreach (var slot in vacantSlots)
            //{
            //    Console.WriteLine("{0} - {1}", slot.Start, slot.End);
            //}
        }

        private static async Task DoProcessing()
        {
            try
            {
                var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

                var recreate = false;
                var reindexAllResources = Boolean.Parse(ConfigurationManager.AppSettings["Search.ReindexAll"]);

                using (var commonHelper = new CommonIndexHelper(connectionString))
                {
                    if (recreate)
                    {
                        //commonHelper.RecreateIndex();
                    }
                    await commonHelper.IndexResources(reindexAllResources);
                }

                using (var personalHelper = new PersonalIndexHelper(connectionString))
                {
                    if (recreate)
                    {
                        //personalHelper.RecreateIndex();
                    }
                    await personalHelper.IndexResources(reindexAllResources);
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
