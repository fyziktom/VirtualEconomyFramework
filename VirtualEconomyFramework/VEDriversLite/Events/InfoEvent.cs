using System;
using System.Collections.Generic;
using System.Text;

namespace VEDriversLite.Events
{
    /// <summary>
    /// Info event object
    /// </summary>
    public class InfoEvent : CommonEvent
    {
        /// <summary>
        /// Create info event object
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        public InfoEvent(string title, string message)
        {
            Type = EventType.Info;
            Title = title;
            Message = message;
            TimeStamp = DateTime.UtcNow;
        }
    }
}
