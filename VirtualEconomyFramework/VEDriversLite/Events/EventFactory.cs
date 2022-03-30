using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.Events
{
    /// <summary>
    /// Get Event based on the type
    /// </summary>
    public static class EventFactory
    {
        /// <summary>
        /// Get Event based on the type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="address"></param>
        /// <param name="txid"></param>
        /// <param name="progress"></param>
        /// <returns></returns>
        public static async Task<IEventInfo> GetEvent(EventType type, string title, string message, string address = "", string txid = "", double progress = 0.0)
        {
            IEventInfo ev = null;
            switch (type)
            {
                case EventType.Info:
                    ev = new InfoEvent(title, message);
                    ev.Address = address;
                    ev.Progress = progress;
                    ev.TxId = txid;
                    return ev;
                case EventType.Error:
                    ev = new ErrorEvent(title, message);
                    ev.Address = address;
                    ev.Progress = progress;
                    ev.TxId = txid;
                    return ev; 
            }

            return null;
        }
        /// <summary>
        /// Clone the event object
        /// </summary>
        /// <param name="inev"></param>
        /// <param name="asType"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static async Task<IEventInfo> Clone(IEventInfo inev, bool asType = false, EventType type = EventType.Info)
        {
            if (!asType)
                type = inev.Type;

            IEventInfo ev = null;
            switch (type)
            {
                case EventType.Info:
                    ev = new InfoEvent(inev.Title, inev.Message);
                    ev.Fill(inev);
                    return ev;
                case EventType.Error:
                    ev = new ErrorEvent(inev.Title, inev.Message);
                    ev.Fill(inev);
                    return ev;
            }
            return null;
        }
    }
}
