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

            var forceStart = false;

            for (var index = 0; index < args.Length; index++)
            {
                switch (args[index].TrimStart('-', '/'))
                {
                    case "register":
                        if (++index >= args.Length)
                        {
                            throw new Exception("Invalid URL after register.");
                        }
                        var urls = args[index].Split(';');

                        if (!new Win32HttpUrlAclService().TryReserve(urls))
                        {
                            Console.WriteLine("HTTP namespace reservation failed.");
                            return 1;
                        }

                        return 0;

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
            using (var webServer = new WebServer(new Win32HttpUrlAclService(), new ScheduleRHttpUrlAclService(), forceStart))
            //using (new Clock(eventStoreRepository, callbackService))
            //using (new SqlServerEventDispatcher(connectionString, (sequenceNumber, @event) => bus.Send(@event)))
            {
                var options = new StartOptions();
                baseUrls.ForEach(options.Urls.Add);

                webServer.Start(options, app => app.UseNancy(o => o.Bootstrapper = bootstrapper));

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
