namespace ScheduleR.Views
{
    using System;
    using System.Collections.Generic;
    using ScheduleR.Model.Events;
    using System.Linq;

    public class ScheduledCallbacksView
    {
        private static readonly long DateTimeEpochTicks = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks;

        private readonly List<Callback> repository;

        public ScheduledCallbacksView(List<Callback> repository)
        {
            Guard.Against.Null(() => repository);

            this.repository = repository;
        }

        public void Consume(CallbackScheduled @event)
        {
            var scheduledDateTime = new DateTime(DateTimeEpochTicks + (@event.EpochMinutes * 60 * 1000 * 10000));

            this.repository.Add(new Callback { Id = @event.Id, ScheduledDateTime = scheduledDateTime });
        }

        public void Consume(CallbackComplete @event)
        {
            var callback = this.repository.Single(cb => cb.Id == @event.Id);
            callback.IsComplete = true;
        }

        public void Consume(CallbackFailed @event)
        {
        }
    }
}
