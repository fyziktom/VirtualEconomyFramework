using System;
using System.Collections.Generic;
using System.Text;

namespace VEDriversLite.Events
{
    public class InfoEvent : CommonEvent
    {
        public InfoEvent(string title, string message)
        {
            Type = EventType.Info;
            Title = title;
            Message = message;
            TimeStamp = DateTime.UtcNow;
        }
    }
}
