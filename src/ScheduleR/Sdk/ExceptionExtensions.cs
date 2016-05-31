namespace ScheduleR.Sdk
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;

    internal static class ExceptionExtensions
    {
        private static Dictionary<int, string> Errors = new Dictionary<int, string>
        {
            { SystemErrorCodes.ERROR_ACCESS_DENIED,  "Unable to start because permission was denied.\r\nRun with the command line parameter -force to attempt to force start by requesting elevation." },
            { SystemErrorCodes.ERROR_ALREADY_EXISTS, "Unable to start because another instance is already running." },
        };

        public static void Handle(this Exception ex, TextWriter writer)
        {
            Guard.Against.Null(() => ex);
            Guard.Against.Null(() => writer);

            var errorCode = -1;

            var httpListnerException = ex as HttpListenerException;
            if (httpListnerException != null)
            {
                errorCode = httpListnerException.ErrorCode;
            }

            string message = null;
            if (!Errors.TryGetValue(errorCode, out message))
            {
                message = "An unexpected error occurred:\r\n{0}";
            }

            writer.WriteLine(message, ex.Message);
            Environment.Exit(errorCode);
        }
    }
}
