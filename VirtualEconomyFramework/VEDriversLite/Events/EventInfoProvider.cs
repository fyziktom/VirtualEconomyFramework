using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.Events
{
    public static class EventInfoProvider
    {
        public static ConcurrentDictionary<string, IEventInfo> EventInfoStore { get; set; } = new ConcurrentDictionary<string, IEventInfo>();
        private static ConcurrentQueue<IEventInfo> infoEventInfoBuffer = new ConcurrentQueue<IEventInfo>();

        private static void AddNewEventInfo(IEventInfo evinfo)
        {
            infoEventInfoBuffer.Enqueue(evinfo);
        }
        private static IEventInfo DequeueEventInfo(IEventInfo evinfo)
        {
            if (infoEventInfoBuffer.TryDequeue(out var ev))
                return ev;
            else
                return null;
        }
        public static string StoreEventInfo(IEventInfo info)
        {
            var guid = Guid.NewGuid();
            EventInfoStore.TryAdd(guid.ToString(), info);
            return guid.ToString();
        }
        public static IEventInfo DeleteEventInfo(string guid)
        {
            if (EventInfoStore.TryRemove(guid, out var info))
                return info;
            else
                return null;
        }
    }
}
