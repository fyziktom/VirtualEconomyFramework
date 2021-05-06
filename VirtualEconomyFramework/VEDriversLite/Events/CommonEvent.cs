using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.Events
{
    public abstract class CommonEvent : IEventInfo
    {
        public EventType Type { get; set; } = EventType.Basic;
        public string Address { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string TxId { get; set; } = string.Empty;
        public string Data { get; set; } = string.Empty;
        public double Progress { get; set; } = 0.0;
        public DateTime TimeStamp { get; set; } = DateTime.UtcNow;

        public virtual void Fill(IEventInfo ev)
        {
            Type = ev.Type;
            Title = ev.Title;
            Message = ev.Message;
            Data = ev.Data;
            Progress = ev.Progress;
            TimeStamp = ev.TimeStamp;
        }
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
