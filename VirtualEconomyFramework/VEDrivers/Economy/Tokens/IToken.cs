using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VEDrivers.Economy;

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
        string IssuerName { get; set; }
        string ImageUrl { get; set; }
        bool MetadataAvailable { get; set; }
        double? ActualBalance { get; set; }

        Task<string> GetDetails();
    }
}
