using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.Events
{
    /// <summary>
    /// Type of the event
    /// </summary>
    public enum EventType
    {
        /// <summary>
        /// Basic
        /// </summary>
        Basic,
        /// <summary>
        /// Info
        /// </summary>
        Info,
        /// <summary>
        /// Warning
        /// </summary>
        Warning,
        /// <summary>
        /// Error
        /// </summary>
        Error,
        /// <summary>
        /// Sending transaction
        /// </summary>
        TxSending,
        /// <summary>
        /// Receiving transaction
        /// </summary>
        TxReceived,
        /// <summary>
        /// NFT Received        
        /// </summary>
        NFTReceived
    }
    /// <summary>
    /// Main interface for the Event infos
    /// </summary>
    public interface IEventInfo
    {
        /// <summary>
        /// Event type
        /// </summary>
        EventType Type { get; set; }
        /// <summary>
        /// Address which created this event info
        /// </summary>
        string Address { get; set; }
        /// <summary>
        /// Title of the event info
        /// </summary>
        string Title { get; set; }
        /// <summary>
        /// Message content
        /// </summary>
        string Message { get; set; }
        /// <summary>
        /// Related transaction
        /// </summary>
        string TxId { get; set; }
        /// <summary>
        /// Related data
        /// </summary>
        string Data { get; set; }
        /// <summary>
        /// Progress of the task which created the event
        /// </summary>
        double Progress { get; set; }
        /// <summary>
        /// Time stamp of the situation
        /// </summary>
        DateTime TimeStamp { get; set; }

        /// <summary>
        /// Fill the dto
        /// </summary>
        /// <param name="ev"></param>
        void Fill(IEventInfo ev);
        /// <summary>
        /// Parse the data of the event
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<T> ParseData<T>();
    }
}
