using System;
using System.Collections.Generic;
using System.Text;

namespace VEDriversLite.Events
{
    /// <summary>
    /// Definition of the ErrorEvent
    /// </summary>
    public class ErrorEvent : CommonEvent
    {
        /// <summary>
        /// construct the event
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        public ErrorEvent(string title, string message)
        {
            Type = EventType.Error;
            Title = title;
            Message = message;
            TimeStamp = DateTime.UtcNow;
        }
    }
}
