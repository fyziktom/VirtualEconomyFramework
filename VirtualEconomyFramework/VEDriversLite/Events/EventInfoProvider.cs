using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.Events
{
    /// <summary>
    /// Store and get infos
    /// </summary>
    public static class EventInfoProvider
    {
        /// <summary>
        /// Storage of the Events
        /// </summary>
        public static ConcurrentDictionary<string, IEventInfo> EventInfoStore { get; set; } = new ConcurrentDictionary<string, IEventInfo>();
        private static ConcurrentQueue<IEventInfo> infoEventInfoBuffer = new ConcurrentQueue<IEventInfo>();

        /// <summary>
        /// Add event to the buffer
        /// </summary>
        /// <param name="evinfo"></param>
        private static void AddNewEventInfo(IEventInfo evinfo)
        {
            infoEventInfoBuffer.Enqueue(evinfo);
        }
        /// <summary>
        /// Get the event from the buffer
        /// </summary>
        /// <param name="evinfo"></param>
        /// <returns></returns>
        private static IEventInfo DequeueEventInfo(IEventInfo evinfo)
        {
            if (infoEventInfoBuffer.TryDequeue(out var ev))
                return ev;
            else
                return null;
        }
        /// <summary>
        /// Save event info to the buffer
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public static string StoreEventInfo(IEventInfo info)
        {
            var guid = Guid.NewGuid();
            EventInfoStore.TryAdd(guid.ToString(), info);
            return guid.ToString();
        }
        /// <summary>
        /// Delete stored info in the buffer
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public static IEventInfo DeleteEventInfo(string guid)
        {
            if (EventInfoStore.TryRemove(guid, out var info))
                return info;
            else
                return null;
        }
    }
}
