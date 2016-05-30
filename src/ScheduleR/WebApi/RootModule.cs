namespace ScheduleR.WebApi
{
    using Nancy;
    using Nancy.Responses;

    public class RootModule : NancyModule
    {
        public RootModule()
        {
            this.Get["/"] = _ =>
                new RedirectResponse(
                    this.Request.Url.BasePath.EndsWith("/") ? "index.html" : string.Concat(this.Request.Url.BasePath, "/index.html"),
                    RedirectResponse.RedirectType.Permanent);

            this.Get["/version"] = _ => new EmbeddedFileResponse(typeof(Program).Assembly, "ScheduleR.Website", "index.html");
        }
    }
}
