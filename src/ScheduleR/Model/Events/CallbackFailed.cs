namespace ScheduleR.Model.Events
{
    using System;

    public class CallbackFailed
    {
        public long EpochMinutes { get; set; }

        public string Id { get; set; }
    }
}
