namespace ScheduleR.Model.Services
{
    using System.Threading.Tasks;

    public class HttpCallbackService : ICallbackService
    {
        public Task<bool> TryInvokeAsync(Callback callback)
        {
            System.Console.WriteLine("Callback: {0}", callback.Id);

            return Task.FromResult(true);
        }
    }
}
