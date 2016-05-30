namespace ScheduleR.Sdk
{
    using System.Collections.Generic;

    internal interface IHttpUrlAclService
    {
        bool TryReserve(IEnumerable<string> urls);
    }
}
