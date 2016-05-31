namespace ScheduleR.Sdk
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;

    internal class ScheduleRHttpUrlAclService : IHttpUrlAclService
    {
        public bool TryReserve(IEnumerable<string> urls)
        {
            Guard.Against.NullOrEmptyOrNullElements(() => urls);

            string args = string.Concat("-register ", string.Join(";", urls));
            var filename = typeof(Program).Assembly.Location;

            // LINK (Cameron): http://stackoverflow.com/questions/2583347/c-sharp-httplistener-without-using-netsh-to-register-a-uri
            var processInfo = new ProcessStartInfo(filename, args)
            {
                Verb = "runas",
                UseShellExecute = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
            };

            try
            {
                var process = Process.Start(processInfo);
                process.WaitForExit();

                return process.ExitCode == 0;
            }
            catch (Win32Exception)
            {
                // NOTE (Cameron): User clicked no to UAC elevation request.
                return false;
            }
        }
    }
}
