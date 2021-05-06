using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.Events
{
    public enum EventType
    {
        Basic,
        Info,
        Warning,
        Error,
        TxSending,
        TxReceived,
        NFTReceived
    }
    public interface IEventInfo
    {
        EventType Type { get; set; }
        string Address { get; set; }
        string Title { get; set; }
        string Message { get; set; }
        string TxId { get; set; }
        string Data { get; set; }
        double Progress { get; set; }
        DateTime TimeStamp { get; set; }

        void Fill(IEventInfo ev);
        Task<T> ParseData<T>();
    }
}
