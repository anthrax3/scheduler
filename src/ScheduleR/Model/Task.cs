namespace ScheduleR.Model
{
    using System;
    using dddlib;

    public class Task : Entity
    {
        public Task(Guid id)
        {
            this.Id = id;
        }

        [NaturalKey]
        public Guid Id { get; private set; }

        PointInTime NextRecurrence { get; set; }
    }
}
