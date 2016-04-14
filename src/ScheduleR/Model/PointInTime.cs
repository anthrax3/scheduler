namespace ScheduleR.Model
{
    using System;
    using dddlib;

    public class PointInTime : ValueObject<PointInTime>
    {
        private static readonly long DateTimeEpochTicks = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks;

        public PointInTime(DateTime dateTime)
            : this(ConvertToEpochMinutes(dateTime))
        {
        }

        public PointInTime(long epochMinutes)
        {
            // NOTE (Cameron): Technically, epoch minutes may be negative to allow for times pre-dating the epoch.
            this.EpochMinutes = epochMinutes;
        }

        public long EpochMinutes { get; private set; }

        public bool IsBefore(DateTime dateTime)
        {
            return this.EpochMinutes < ConvertToEpochMinutes(dateTime);
        }

        private static long ConvertToEpochMinutes(DateTime dateTime)
        {
            if (dateTime.Kind != DateTimeKind.Utc)
            {
                throw new BusinessException("The DateTime.Kind for a point in time must be UTC.");
            }

            return (dateTime.Ticks - DateTimeEpochTicks) / (60 * 1000 * 10000);
        }
    }
}
