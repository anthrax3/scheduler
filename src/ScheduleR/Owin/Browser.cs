namespace Owin
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;

    internal sealed class Browser
    {
        public void Open(string url)
        {
            Guard.Against.Null(() => url);

            // NOTE (Cameron): Supplied URL may be a URL prefix so we need to fix this.
            var actualUrl = url.Replace("+", "localhost").Replace("*", "localhost");
            var uri = new Uri(actualUrl, UriKind.Absolute);

            if (!Environment.UserInteractive)
            {
                return;
            }

            // TODO (Cameron): Move try...catch from here.
            try
            {
                // LINK (Cameron): http://stackoverflow.com/questions/5191411/why-is-usingnull-a-valid-case-in-c
                using (Process.Start(uri.ToString()))
                {
                }
            }
            catch (Win32Exception ex)
            {
                if (ex.ErrorCode == -2147467259)
                {
                    // noBrowser.Message
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
