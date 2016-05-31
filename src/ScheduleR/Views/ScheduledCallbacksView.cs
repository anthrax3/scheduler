namespace ScheduleR.Views
{
    using System;
    using dddlib.Projections;
    using ScheduleR.Model.Events;

    public class ScheduledCallbacksView
    {
        private static readonly long DateTimeEpochTicks = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks;

        private readonly IRepository<string, Callback> repository;

        public ScheduledCallbacksView(IRepository<string, Callback> repository)
        {
            Guard.Against.Null(() => repository);

            this.repository = repository;
        }

        public void Consume(CallbackScheduled @event)
        {
            var scheduledDateTime = new DateTime(DateTimeEpochTicks + (@event.EpochMinutes * 60 * 1000 * 10000));

            this.repository.AddOrUpdate(
                @event.Id,
                new Callback
                {
                    Id = @event.Id,
                    ScheduledDateTime = scheduledDateTime
                });
        }

        public void Consume(CallbackComplete @event)
        {
            var callback = this.repository.Get(@event.Id);
            callback.IsComplete = true;
            this.repository.AddOrUpdate(@event.Id, callback);
        }

        public void Consume(CallbackFailed @event)
        {
            var callback = this.repository.Get(@event.Id);
            callback.IsFailed = true;
            this.repository.AddOrUpdate(@event.Id, callback);
        }
    }
}
