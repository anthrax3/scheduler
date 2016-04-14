namespace ScheduleR.Application
{
    using System;
    using System.Threading;
    using dddlib.Persistence;
    using ScheduleR.Model;

    public sealed class Clock : IDisposable
    {
        private readonly Timer timer;
        private readonly IEventStoreRepository repository;

        public Clock(IEventStoreRepository repository)
        {
            Guard.Against.Null(() => repository);

            this.repository = repository;

            this.timer = new Timer(
                state =>
                {
                    try
                    {
                        this.Tick(new PointInTime(DateTime.UtcNow));
                    }
                    catch (Exception)
                    {
                        // TODO (Cameron): Log?
                    }

                    this.timer.Change(60 * 1000, Timeout.Infinite);
                },
                null,
                30 * 1000,
                Timeout.Infinite);
        }

        public void Dispose()
        {
            if (this.timer != null)
            {
                this.timer.Dispose();
            }
        }

        private void Tick(PointInTime pointInTime)
        {
            Console.WriteLine("Tick {0}", pointInTime.EpochMinutes);

            var schedule = default(Schedule);
            try
            {
                schedule = this.repository.Load<Schedule>(pointInTime);
            }
            catch (Exception)
            {
                return;
            }

            schedule.Compete();
        }
    }
}
