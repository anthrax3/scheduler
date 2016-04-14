namespace ScheduleR
{
    using System;
    using System.Configuration;
    using dddlib.Persistence.SqlServer;
    using ScheduleR.Application;
    using ScheduleR.Model;

    internal class Program
    {
        public static void Main(string[] args)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["ScheduleR"].ConnectionString;
            var repository = new SqlServerEventStoreRepository(connectionString, "ScheduleR");

            // add scheduled notification
            var pointInTime = new PointInTime(DateTime.UtcNow.AddMinutes(2));
            var schedule = new Schedule(pointInTime);
            var task = new Task(Guid.NewGuid());
            schedule.Add(task);
            repository.Save(schedule);

            using (new Clock(repository))
            {
                Console.WriteLine("Waiting...");
                Console.ReadLine();
            }
        }
    }
}
