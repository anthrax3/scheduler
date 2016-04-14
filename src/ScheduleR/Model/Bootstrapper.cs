namespace ScheduleR.Model
{
    using System.Globalization;
    using dddlib.Configuration;
    using ScheduleR.Model.Events;

    internal class Bootstrapper : IBootstrapper
    {
        public void Bootstrap(IConfiguration configure)
        {
            configure.AggregateRoot<Schedule>().ToReconstituteUsing(() => new Schedule());

            configure.ValueObject<PointInTime>()
                .ToMapToEvent<ScheduleCreated>(
                    (pointInTime, @event) => @event.PointInTimeEpochMinutes = pointInTime.EpochMinutes,
                    @event => new PointInTime(@event.PointInTimeEpochMinutes))
                .ToUseValueObjectSerializer(
                    pointInTime => pointInTime.EpochMinutes.ToString(),
                    epochMinutes => new PointInTime(long.Parse(epochMinutes, CultureInfo.InvariantCulture)));

            configure.Entity<Task>().ToMapToEvent<ScheduleTaskAdded>(
                (task, @event) => @event.TaskId = task.Id,
                @event => new Task(@event.TaskId));
        }
    }
}
