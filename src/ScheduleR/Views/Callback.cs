namespace ScheduleR.Views
{
    using System;

    public class Callback
    {
        public string Id { get; set; }

        public DateTime ScheduledDateTime { get; set; }

        public bool IsComplete { get; set; }

        public bool IsFailed { get; set; }
    }
}
