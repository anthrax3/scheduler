namespace ScheduleR.Model
{
    using System.Collections.Generic;
    using dddlib;

    public class Message : ValueObject<Message>
    {
        //private readonly IDictionary<string, object> content;

        public Message(IDictionary<string, object> content)
        {
            // TODO (Cameron): Copy the content.
        }
    }
}
