namespace ScheduleR.WebApi
{
    using System.Globalization;
    using Nancy;
    using Nancy.ErrorHandling;
    using Nancy.Responses;
    using Nancy.ViewEngines;

    public class CustomErrorHandler : DefaultViewRenderer, IStatusCodeHandler
    {
        public CustomErrorHandler(IViewFactory factory)
            : base(factory)
        {
        }

        public bool HandlesStatusCode(HttpStatusCode statusCode, NancyContext context)
        {
            return statusCode == HttpStatusCode.NotFound || statusCode == HttpStatusCode.InternalServerError;
        }

        public void Handle(HttpStatusCode statusCode, NancyContext context)
        {
            var response = new EmbeddedFileResponse(typeof(Program).Assembly, "ScheduleR.Website", string.Concat(((int)statusCode).ToString(CultureInfo.InvariantCulture), ".html"));
            response.StatusCode = statusCode;
            context.Response = response;
        }
    }
}
