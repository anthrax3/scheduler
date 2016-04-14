namespace ScheduleR.Model.Events
{
    using System;

    public class CallbackComplete
    {
        public long EpochMinutes { get; set; }

        public string Id { get; set; }
    }
}
