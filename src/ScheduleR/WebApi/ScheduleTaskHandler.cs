namespace ScheduleR.WebApi
{
    using dddlib.Persistence;
    using dddlib.Projections;
    using Nancy;
    using Views;

    public class ScheduleTaskModule : NancyModule
    {
        public ScheduleTaskModule(IEventStoreRepository eventStoreRepository, IRepository<string, Callback> viewRepository)
        {
            Guard.Against.Null(() => eventStoreRepository);
            Guard.Against.Null(() => viewRepository);

            //this.Get["/"] = _ => new GenericFileResponse("Content/index.html", "text/html");

            this.Post["/task"] = _ =>
            {
                //var epochMinutes = new EpochMinutes(at.ToUniversalTime());
                //var pointInTime = new PointInTime(epochMinutes);
                //var callback = new Callback(id, url);
                //pointInTime.Schedule(callback);
                //repository.Save(pointInTime);

                return new Response { StatusCode = HttpStatusCode.OK };
            };
        }
    }
}
