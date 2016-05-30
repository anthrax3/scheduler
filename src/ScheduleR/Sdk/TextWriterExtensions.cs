namespace ScheduleR.Sdk
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Reflection;

    internal static class TextWriterExtensions
    {
        private static Dictionary<int, string> Errors = new Dictionary<int, string>
        {
            { SystemErrorCodes.ERROR_ACCESS_DENIED,  "Unable to start because permission was denied.\r\nRun with the command line parameter -force to attempt to force start by requesting elevation." },
            { SystemErrorCodes.ERROR_ALREADY_EXISTS, "Unable to start because another instance is already running." },
        };

        public static void Log(this TextWriter writer, Assembly assembly)
        {
            var title = assembly.Attribute<AssemblyTitleAttribute>(e => e.Title);
            var copyright = assembly.Attribute<AssemblyCopyrightAttribute>(e => e.Copyright);
            var version = assembly.Attribute<AssemblyInformationalVersionAttribute>(e => e.InformationalVersion);

            writer.WriteLine("{0} [{1}]", title, version);
            writer.WriteLine(copyright);
        }

        public static void LogAndTerminate(this TextWriter writer, Exception ex)
        {
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

        private static string Attribute<T>(this ICustomAttributeProvider provider, Func<T, string> property)
        {
            var value = provider.GetCustomAttributes(typeof(T), false).Cast<T>().FirstOrDefault();
            return value == null ? string.Empty : property(value);
        }
    }
}
