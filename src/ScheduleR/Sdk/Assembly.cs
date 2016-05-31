namespace ScheduleR.Sdk
{
    using System;
    using System.Linq;
    using System.Reflection;

    public static class Assembly
    {
        private static string Attribute<T>(this ICustomAttributeProvider provider, Func<T, string> property)
        {
            var value = provider.GetCustomAttributes(typeof(T), false).Cast<T>().FirstOrDefault();
            return value == null ? string.Empty : property(value);
        }

        public class Information
        {
            public Information(System.Reflection.Assembly assembly)
            {
                Guard.Against.Null(() => assembly);

                this.Title = assembly.Attribute<AssemblyTitleAttribute>(e => e.Title);
                this.Copyright = assembly.Attribute<AssemblyCopyrightAttribute>(e => e.Copyright);
                this.Version = assembly.Attribute<AssemblyInformationalVersionAttribute>(e => e.InformationalVersion);
            }
                
            public string Title { get; private set; }

            public string Copyright { get; private set; }

            public string Version { get; private set; }
        }
    }
}
