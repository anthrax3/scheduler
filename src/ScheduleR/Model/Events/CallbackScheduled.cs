namespace ScheduleR.Model.Events
{
    using System;

    public class CallbackScheduled
    {
        public long EpochMinutes { get; set; }

        public string Id { get; set; }

        public Uri Url { get; set; }
    }
}
