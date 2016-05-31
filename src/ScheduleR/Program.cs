namespace ScheduleR
{
    using System;
    using System.Configuration;
    using System.Linq;
    using dddlib.Persistence.SqlServer;
    using dddlib.Projections.Memory;
    using Microsoft.Owin.Hosting;
    using Owin;
    using ScheduleR.Model.Services;
    using Sdk;
    using Views;
    using WebApi;

    internal class Program
    {
        private static string DefaultUrlPrefix = "http://+:80/ScheduleR";

        public static int Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, e) => ((Exception)e.ExceptionObject).Handle(Console.Out);

            var assemblyInformation = new Assembly.Information(typeof(Program).Assembly);
            Console.WriteLine("{0} [{1}]\r\n{2}", assemblyInformation.Title, assemblyInformation.Version, assemblyInformation.Copyright);
            Console.WriteLine();

            WebServer.AutoRegisterUrls();

            var forceStart = false;

            for (var index = 0; index < args.Length; index++)
            {
                switch (args[index].TrimStart('-', '/'))
                {
                    case "f":
                    case "force":
                    case "e":
                    case "elevate":
                        forceStart = true;
                        break;
                }
            }

            var connectionString = ConfigurationManager.ConnectionStrings["ScheduleR"].ConnectionString;
            var baseUrls = (ConfigurationManager.AppSettings["BaseUrls"] ?? DefaultUrlPrefix)
                .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                .ToList();

            var eventStoreRepository = new SqlServerEventStoreRepository(connectionString);

            var callbackService = new HttpCallbackService();

            var viewRepository = new MemoryRepository<string, Callback>();
            var view = new ScheduledCallbacksView(viewRepository);

            var bus = new Microbus().AutoRegister(view);

            // event replay
            var eventStore = new dddlib.Projections.SqlServer.SqlServerEventStore(connectionString);
            foreach (var @event in eventStore.GetEventsFrom(0))
            {
                bus.Send(@event);
            }

            using (var bootstrapper = new NancyBootstrapper(eventStoreRepository, viewRepository))
            using (var webServer = new WebServer(forceStart).Start(baseUrls, app => app.UseNancy(o => o.Bootstrapper = bootstrapper)))
            //using (new Clock(eventStoreRepository, callbackService))
            //using (new SqlServerEventDispatcher(connectionString, (sequenceNumber, @event) => bus.Send(@event)))
            {
                Console.WriteLine("Listening on:");
                baseUrls.ForEach(Console.WriteLine);

                if (Environment.UserInteractive)
                {
                    var baseUrl = baseUrls.First().Replace("+", "localhost").Replace("*", "localhost");

                    new Browser().Open(new Uri(baseUrl));
                }

                string line;
                do
                {
                    foreach (var item in viewRepository.GetAll())
                    {
                        var callback = item.Value;
                        Console.WriteLine(
                            "Callback '{0}' scheduled for {1} (Complete = {2})",
                            callback.Id,
                            callback.ScheduledDateTime,
                            callback.IsComplete);
                    }

                    line = Console.ReadLine();
                }
                while (!"exit".Equals(line, StringComparison.OrdinalIgnoreCase));
            }

            return 0;
        }
    }
}
