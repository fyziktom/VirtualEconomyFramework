using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.Events
{
    public static class EventFactory
    {
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
