namespace ScheduleR.Model
{
    using System;
    using System.Collections.Generic;
    using dddlib;
    using ScheduleR.Model.Events;

    public class Schedule : AggregateRoot
    {
        private readonly List<Task> scheduledTasks = new List<Task>();

        protected internal Schedule()
        {
        }

        public Schedule(PointInTime pointInTime)
        {
            Guard.Against.Null(() => pointInTime);

            if (pointInTime.IsBefore(DateTime.UtcNow))
            {
                throw new BusinessException("Cannot create a schedule for a point in time that occurs in the past.");
            }
            
            this.Apply(Map.ValueObject(pointInTime).ToEvent<ScheduleCreated>());
        }

        [NaturalKey]
        public PointInTime PointInTime { get; private set; }

        public void Add(Task task)
        {
            Guard.Against.Null(() => task);

            if (this.scheduledTasks.Contains(task))
            {
                throw new BusinessException("The specified task has already been scheduled.");
            }

            var @event = new ScheduleTaskAdded { PointInTimeEpochMinutes = this.PointInTime.EpochMinutes };

            Map.Entity(task).ToEvent(@event);

            this.Apply(@event);
        }

        public void Compete()
        {
            Console.WriteLine("Completed schedule for {0}", this.PointInTime.EpochMinutes);
        }

        private void Handle(ScheduleCreated @event)
        {
            this.PointInTime = Map.Event(@event).ToValueObject<PointInTime>();
        }

        private void Handle(ScheduleTaskAdded @event)
        {
            this.scheduledTasks.Add(Map.Event(@event).ToEntity<Task>());
        }
    }
}
