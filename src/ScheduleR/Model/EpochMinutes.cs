namespace ScheduleR.Model
{
    using System;
    using dddlib;

    public sealed class EpochMinutes : ValueObject<EpochMinutes>
    {
        private static readonly long DateTimeEpochTicks = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks;

        public EpochMinutes(DateTime dateTime)
            : this(ConvertToEpochMinutes(dateTime))
        {
        }

        public EpochMinutes(long value)
        {
            this.Value = value;
        }

        public long Value { get; private set; }

        public bool IsBefore(DateTime dateTime)
        {
            return this.Value < ConvertToEpochMinutes(dateTime);
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
