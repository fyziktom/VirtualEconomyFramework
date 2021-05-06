using System;
using System.Collections.Generic;
using System.Text;

namespace VEDriversLite.Events
{
    public class ErrorEvent : CommonEvent
    {
        public ErrorEvent(string title, string message)
        {
            Type = EventType.Error;
            Title = title;
            Message = message;
            TimeStamp = DateTime.UtcNow;
        }
    }
}
