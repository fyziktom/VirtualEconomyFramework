using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VEDrivers.Economy;
using VEDrivers.Economy.Transactions;

namespace VEDrivers.Economy.Tokens
{
    public enum TokenTypes
    {
        Common,
        NTP1
    }
    public interface IToken : IUnitBase
    {
        string Id { get; set; }
        string TxId { get; set; }
        string IssuerName { get; set; }
        string ImageUrl { get; set; }
        string To { get; set; }
        string From { get; set; }
        bool MetadataAvailable { get; set; }
        double? ActualBalance { get; set; }
        TransactionDirection Direction { get; set; }
        DateTime TimeStamp { get; set; }
        Dictionary<string, string> Metadata { get; set; }
        Task<string> GetDetails();
    }
}
