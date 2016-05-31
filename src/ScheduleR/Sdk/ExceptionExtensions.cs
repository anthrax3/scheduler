namespace ScheduleR.Sdk
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Net;

    internal static class ExceptionExtensions
    {
        private static Dictionary<SystemErrorCodes, string> Errors = new Dictionary<SystemErrorCodes, string>
        {
            { SystemErrorCodes.ERROR_ACCESS_DENIED,  "Unable to start because permission was denied.\r\nRun with the command line parameter -force to attempt to force start by requesting elevation." },
            { SystemErrorCodes.ERROR_ALREADY_EXISTS, "Unable to start because another instance is already running." },
        };

        public static void Handle(this Exception ex, TextWriter writer)
        {
            Guard.Against.Null(() => ex);
            Guard.Against.Null(() => writer);

            var errorCode = SystemErrorCodes.Undefined;

            var httpListnerException = ex as HttpListenerException;
            if (httpListnerException != null)
            {
                if (!Enum.TryParse(httpListnerException.ErrorCode.ToString(CultureInfo.InvariantCulture), out errorCode))
                {
                    errorCode = SystemErrorCodes.Undefined;
                }
            }

            string message = null;
            if (!Errors.TryGetValue(errorCode, out message))
            {
                message = "An unexpected error occurred:\r\n{0}";
            }

            writer.WriteLine(message, ex.Message);
            Environment.Exit((int)errorCode);
        }

        // LINK (Cameron): https://msdn.microsoft.com/en-us/library/ms681382(v=vs.85).aspx
        private enum SystemErrorCodes
        {
            Undefined = -1,
            ERROR_ACCESS_DENIED = 5,
            ERROR_ALREADY_EXISTS = 183,
        }
    }
}
