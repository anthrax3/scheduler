namespace ScheduleR.Model.Events
{
    using System;

    public class ScheduleTaskAdded
    {
        public long PointInTimeEpochMinutes { get; set; }

        public Guid TaskId { get; set; }
    }
}
