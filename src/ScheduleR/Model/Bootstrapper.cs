namespace ScheduleR.Model
{
    using System.Globalization;
    using dddlib.Configuration;
    using ScheduleR.Model.Events;

    internal class Bootstrapper : IBootstrapper
    {
        public void Bootstrap(IConfiguration configure)
        {
            configure.AggregateRoot<PointInTime>().ToReconstituteUsing(() => new PointInTime());

            configure.ValueObject<EpochMinutes>()
                .ToMapToEvent<CallbackScheduled>(
                    (epochMinutes, @event) => @event.EpochMinutes = epochMinutes.Value,
                    @event => new EpochMinutes(@event.EpochMinutes))
                .ToUseValueObjectSerializer(
                    epochMinutes => epochMinutes.Value.ToString(CultureInfo.InvariantCulture),
                    epochMinutesValue => new EpochMinutes(long.Parse(epochMinutesValue, CultureInfo.InvariantCulture)));

            configure.Entity<Callback>().ToMapToEvent<CallbackScheduled>(
                (task, @event) => @event.Id = task.Id,
                @event => new Callback(@event.Id));
        }
    }
}
