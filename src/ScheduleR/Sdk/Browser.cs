namespace ScheduleR.Sdk
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;

    internal sealed class Browser
    {
        public void Open(Uri uri)
        {
            Guard.Against.Null(() => uri);

            if (!uri.IsAbsoluteUri)
            {
                throw new ArgumentException("Invalid URI specified.", Guard.Expression.Parse(() => uri));
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
                //MessageBox.Show(other.Message);
            }
        }
    }
}
