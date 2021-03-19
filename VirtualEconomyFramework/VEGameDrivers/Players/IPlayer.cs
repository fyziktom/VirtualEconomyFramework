using System;
using System.Collections.Generic;
using System.Text;

namespace VEGameDrivers.Players
{
    public enum PlayerTypes
    {
        BasicPlayer,
        ChessPlayer
    }

    public interface IPlayer
    {
        PlayerTypes Type { get; set; }
        string Address { get; set; }
        string Name { get; set; }
        string NickName { get; set; }
        string Email { get; set; }
        double Score { get; set; }
    }
}
