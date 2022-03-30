using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.Events
{
    /// <summary>
    /// Basic implementation of the Event interface.
    /// </summary>
    public abstract class CommonEvent : IEventInfo
    {
        /// <summary>
        /// Event type
        /// </summary>
        public EventType Type { get; set; } = EventType.Basic;
        /// <summary>
        /// Address which created this event info
        /// </summary>
        public string Address { get; set; } = string.Empty;
        /// <summary>
        /// Title of the event info
        /// </summary>
        public string Title { get; set; } = string.Empty;
        /// <summary>
        /// Message content
        /// </summary>
        public string Message { get; set; } = string.Empty;
        /// <summary>
        /// Related transaction
        /// </summary>
        public string TxId { get; set; } = string.Empty;
        /// <summary>
        /// Related data
        /// </summary>
        public string Data { get; set; } = string.Empty;
        /// <summary>
        /// Progress of the task which created the event
        /// </summary>
        public double Progress { get; set; } = 0.0;
        /// <summary>
        /// Time stamp of the situation
        /// </summary>
        public DateTime TimeStamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Fill the dto
        /// </summary>
        /// <param name="ev"></param>
        public virtual void Fill(IEventInfo ev)
        {
            Type = ev.Type;
            Title = ev.Title;
            Message = ev.Message;
            Data = ev.Data;
            Progress = ev.Progress;
            TimeStamp = ev.TimeStamp;
        }
        /// <summary>
        /// Parse the data of the event
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<T> ParseData<T>()
        {
            try
            {
                if (!string.IsNullOrEmpty(Data))
                {
                    var data = JsonConvert.DeserializeObject<T>(Data);
                    return data;
                }
                else
                {
                    throw new Exception("Cannot deserialize Event data. Data field is empty.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Cannot deserialize Event data. " + ex.Message);
            }
        }
    }
}
