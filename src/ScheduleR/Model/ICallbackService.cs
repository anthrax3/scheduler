namespace ScheduleR.Model
{
    using System;
    using System.Threading.Tasks;

    public interface ICallbackService
    {
        Task<bool> TryInvokeAsync(Callback callback);
    }
}
