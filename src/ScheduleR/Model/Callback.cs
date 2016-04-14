namespace ScheduleR.Model
{
    using dddlib;

    public class Callback : Entity
    {
        public Callback(string id)
        {
            Guard.Against.Null(() => id);

            this.Id = id;
        }

        [NaturalKey]
        public string Id { get; private set; }
    }
}
