namespace ScheduleR.Sdk
{
    using System;
    using System.Net;
    using System.Reflection;
    using System.Runtime.ExceptionServices;
    using Microsoft.Owin.Hosting;
    using Owin;

    internal class WebServer : IDisposable
    {
        private readonly IHttpUrlAclService defaultHttpUrlAclService;
        private readonly IHttpUrlAclService elevatedHttpUrlAclService;
        private readonly bool forceStart;

        private IDisposable webServer;

        public WebServer(IHttpUrlAclService defaultHttpUrlAclService, IHttpUrlAclService elevatedHttpUrlAclService, bool forceStart)
        {
            Guard.Against.Null(() => defaultHttpUrlAclService);
            Guard.Against.Null(() => elevatedHttpUrlAclService);

            this.defaultHttpUrlAclService = defaultHttpUrlAclService;
            this.elevatedHttpUrlAclService = elevatedHttpUrlAclService;
            this.forceStart = forceStart;
        }

        public void Start(StartOptions options, Action<IAppBuilder> startup)
        {
            Guard.Against.Null(() => options);

            this.webServer = this.GetWebServer(options, startup);
        }

        public void Stop()
        {
            if (this.webServer == null)
            {
                return;
            }

            this.webServer.Dispose();
            this.webServer = null;
        }

        public void Dispose()
        {
            this.Stop();
        }

        private IDisposable GetWebServer(StartOptions options, Action<IAppBuilder> startup)
        {
            this.defaultHttpUrlAclService.TryReserve(options.Urls);

            HttpListenerException httpListnerException;
            try
            {
                return WebApp.Start(options, startup);
            }
            catch (TargetInvocationException ex)
            {
                httpListnerException = ex.InnerException as HttpListenerException;
                if (httpListnerException == null)
                {
                    throw;
                }
            }

            if (httpListnerException.ErrorCode == SystemErrorCodes.ERROR_ACCESS_DENIED &&
                this.forceStart &&
                this.elevatedHttpUrlAclService.TryReserve(options.Urls))
            {
                return WebApp.Start(options, startup);
            }

            ExceptionDispatchInfo.Capture(httpListnerException).Throw();

            // HACK (Cameron): Unreachable code.
            return null;
        }
    }
}
