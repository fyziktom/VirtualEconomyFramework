using System;
using VEDriversLite.NeblioAPI;

public enum TxWay
{
    In,
    Out,
    Sub,
}

public class TxDetails
{
    public GetTransactionInfoResponse Info { get; set; } = new GetTransactionInfoResponse();
    public string Sender { get; set; } = string.Empty;
    public string Receiver { get; set; } = string.Empty;
    public bool FromAnotherAccount { get; set; } = false;
    public bool FromSubAccount { get; set; } = false;
    public DateTime Time { get; set; } = DateTime.MinValue;
    public TxWay Way
    {
        get
        {
            if (FromAnotherAccount)
                return TxWay.In;
            else if (FromAnotherAccount)
                return TxWay.Sub;
            return TxWay.Out;
        }
    }
}
