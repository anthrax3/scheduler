namespace ScheduleR
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using dddlib.Persistence.EventDispatcher.SqlServer;
    using dddlib.Persistence.SqlServer;
    using Model.Events;
    using ScheduleR.Application;
    using ScheduleR.Model;
    using ScheduleR.Model.Services;
    using Views;

    internal class Program
    {
        public static void Main(string[] args)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["ScheduleR"].ConnectionString;
            //var repository = new MemoryEventStoreRepository();
            var repository = new SqlServerEventStoreRepository(connectionString);

            // add scheduled notification
            var epochMinutes = new EpochMinutes(DateTime.UtcNow.AddMinutes(2));
            var pointInTime = new PointInTime(epochMinutes);
            var callback = new Model.Callback("test");
            pointInTime.Schedule(callback);
            repository.Save(pointInTime);

            //pointInTime.Schedule(callback);
            /*
                schedule a task to occur at a point in time
                task.Schedule(point in time)
             * 
             * */
            var service = new HttpCallbackService();

            var viewRepository = new List<Views.Callback>();
            var view = new ScheduledCallbacksView(viewRepository);

            var bus = new Microbus();
            bus.Register<CallbackScheduled>(view.Consume);
            bus.Register<CallbackComplete>(view.Consume);
            bus.Register<CallbackFailed>(view.Consume);

            using (new SqlServerEventDispatcher(connectionString, (sequenceNumber, @event) => bus.Send(@event)))
            using (new Clock(repository, service))
            {
                Console.WriteLine("Waiting...");

                string line;
                do
                {
                    line = Console.ReadLine();
                    viewRepository.ForEach(x => Console.WriteLine("Callback '{0}' scheduled for {1} (Complete = {2})", x.Id, x.ScheduledDateTime, x.IsComplete));
                }
                while (!"exit".Equals(line, StringComparison.OrdinalIgnoreCase));

            }
        }
    }
}
