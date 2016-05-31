namespace Owin
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using System.Runtime.ExceptionServices;
    using Microsoft.Owin.Hosting;

    internal class WebServer : IDisposable
    {
        private const string UrlPrefixRegisterArg = "register";

        private static readonly Win32HttpUrlAclService HttpUrlAclService = new Win32HttpUrlAclService();

        private static bool AutoRegisterInvoked;

        private readonly bool? forceStart;

        private IDisposable webServer;

        public WebServer()
        {
        }

        public WebServer(bool forceStart)
        {
            this.forceStart = forceStart;
        }

        public WebServer Start(IEnumerable<string> urls, Action<IAppBuilder> startup)
        {
            var options = new StartOptions();
            urls.ToList().ForEach(options.Urls.Add);

            return this.Start(options, startup);
        }

        public WebServer Start(StartOptions options, Action<IAppBuilder> startup)
        {
            Guard.Against.Null(() => options);

            if (this.webServer != null)
            {
                throw new NotSupportedException("Cannot run more than one web server concurrently.");
            }

            this.webServer = this.GetWebServer(options, startup);

            return this;
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

        public static void AutoRegisterUrls()
        {
            AutoRegisterInvoked = true;

            var args = Environment.GetCommandLineArgs();
            for (var index = 0; index < args.Length; index++)
            {
                switch (args[index].TrimStart('-', '/'))
                {
                    case UrlPrefixRegisterArg:
                        if (++index >= args.Length)
                        {
                            throw new Exception("Invalid URL after register.");
                        }
                        var urls = args[index].Split(';');

                        var errorCode = 0;
                        if (!HttpUrlAclService.TryReserve(urls, out errorCode))
                        {
                            Console.WriteLine("HTTP namespace reservation failed.");
                        }

                        Environment.Exit(errorCode);
                        break;
                }
            }

        }

        private IDisposable GetWebServer(StartOptions options, Action<IAppBuilder> startup)
        {
            if (this.forceStart.HasValue && !AutoRegisterInvoked)
            {
                throw new NotSupportedException("Cannot force a web server start without first calling WebServer.AutoRegister().");
            }

            var errorCode = 0;
            HttpUrlAclService.TryReserve(options.Urls, out errorCode);

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

            if (httpListnerException.ErrorCode == 5 /* ERROR_ACCESS_DENIED */ &&
                this.forceStart.HasValue && this.forceStart.Value &&
                this.TryElevatedReserve(options.Urls, out errorCode))
            {
                return WebApp.Start(options, startup);
            }

            ExceptionDispatchInfo.Capture(httpListnerException).Throw();

            // HACK (Cameron): Unreachable code.
            return null;
        }

        private bool TryElevatedReserve(IEnumerable<string> urls, out int errorCode)
        {
            string args = string.Concat("-", UrlPrefixRegisterArg, " ", string.Join(";", urls));
            var filename = Assembly.GetExecutingAssembly().Location;

            // LINK (Cameron): http://stackoverflow.com/questions/2583347/c-sharp-httplistener-without-using-netsh-to-register-a-uri
            var processInfo = new ProcessStartInfo(filename, args)
            {
                Verb = "runas",
                UseShellExecute = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
            };

            Process process;
            try
            {
                process = Process.Start(processInfo);
            }
            catch (Win32Exception)
            {
                // NOTE (Cameron): User clicked no to UAC elevation request.
                errorCode = -1;
                return false;
            }

            process.WaitForExit();

            errorCode = process.ExitCode;
            return process.ExitCode == 0;
        }
    }
}
