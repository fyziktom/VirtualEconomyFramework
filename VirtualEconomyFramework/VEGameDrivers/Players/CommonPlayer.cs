using System;
using System.Collections.Generic;
using System.Text;

namespace VEGameDrivers.Players
{
    public abstract class CommonPlayer : IPlayer
    {
        public PlayerTypes Type { get; set; } = PlayerTypes.BasicPlayer;
        public string Address { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string NickName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public double Score { get; set; } = 0.0;
    }
}
