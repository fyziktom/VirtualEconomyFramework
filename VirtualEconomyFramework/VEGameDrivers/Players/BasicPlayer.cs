using System;
using System.Collections.Generic;
using System.Text;

namespace VEGameDrivers.Players
{
    public class BasicPlayer : CommonPlayer
    {
        public BasicPlayer(string address, string name, string nickname, string email, double score)
        {
            Address = address;
            Name = name;
            NickName = nickname;
            Email = email;
            Score = score;
            Type = PlayerTypes.BasicPlayer;
        }

    }
}
