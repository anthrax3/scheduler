namespace ScheduleR.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using dddlib;
    using ScheduleR.Model.Events;

    public class PointInTime : AggregateRoot
    {
        private readonly List<Callback> scheduledCallbacks = new List<Callback>();

        protected internal PointInTime()
        {
        }

        public PointInTime(EpochMinutes epochMinutes)
        {
            Guard.Against.Null(() => epochMinutes);

            if (epochMinutes.IsBefore(DateTime.UtcNow))
            {
                throw new BusinessException("Cannot create a schedule for a point in time that occurs in the past.");
            }

            // NOTE (Cameron): This is very odd, I know.
            this.EpochMinutes = epochMinutes;
        }

        [NaturalKey]
        public EpochMinutes EpochMinutes { get; private set; }

        public void Schedule(Callback callback)
        {
            Guard.Against.Null(() => callback);

            if (this.scheduledCallbacks.Contains(callback))
            {
                throw new BusinessException("The specified task has already been scheduled.");
            }

            this.Apply(Map.Entity(callback).ToEvent(new CallbackScheduled { EpochMinutes = this.EpochMinutes.Value }));
        }

        public void InvokeCallbacks(ICallbackService callbackService)
        {
            var callbacks = this.scheduledCallbacks
                .Select(async callback => 
                    this.Apply(
                        await callbackService.TryInvokeAsync(callback)
                            ? (object)new CallbackComplete { EpochMinutes = this.EpochMinutes.Value , Id = callback.Id }
                            : new CallbackFailed { EpochMinutes = this.EpochMinutes.Value, Id = callback.Id }))
                .ToArray();

            Task.WaitAll(callbacks);
        }

        private void Handle(CallbackScheduled @event)
        {
            if (this.EpochMinutes == null)
            {
                this.EpochMinutes = new EpochMinutes(@event.EpochMinutes);
            }
             
            this.scheduledCallbacks.Add(Map.Event(@event).ToEntity<Callback>());
        }
    }
}
