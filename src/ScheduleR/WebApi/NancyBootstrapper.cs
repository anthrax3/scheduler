namespace ScheduleR.WebApi
{
    using dddlib.Persistence;
    using dddlib.Projections;
    using Nancy;
    using Nancy.Conventions;
    using Nancy.Embedded.Conventions;
    using Nancy.TinyIoc;
    using Views;

    public class NancyBootstrapper : DefaultNancyBootstrapper
    {
        private readonly IEventStoreRepository eventStoreRepository;
        private readonly IRepository<string, Callback> viewRepository;

        public NancyBootstrapper(IEventStoreRepository eventStoreRepository, IRepository<string, Callback> viewRepository)
        {
            Guard.Against.Null(() => eventStoreRepository);
            Guard.Against.Null(() => viewRepository);

            this.eventStoreRepository = eventStoreRepository;
            this.viewRepository = viewRepository;
        }

        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            base.ConfigureApplicationContainer(container);

            container.Register(this.eventStoreRepository);
            container.Register(this.viewRepository);
        }

        protected override void ConfigureConventions(NancyConventions nancyConventions)
        {

            nancyConventions.StaticContentsConventions.Add(
                EmbeddedStaticContentConventionBuilder.AddDirectory("/", typeof(Program).Assembly, "Website"));

            base.ConfigureConventions(nancyConventions);
        }
    }
}
