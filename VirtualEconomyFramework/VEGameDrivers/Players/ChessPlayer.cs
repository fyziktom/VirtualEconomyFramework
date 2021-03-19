using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEGameDrivers.Players
{
    public enum FigureTypes
    {
        Black,
        White
    }
    public class ChessPlayer : CommonPlayer
    {
        public ChessPlayer(string address, string name, string nickname, string email, double score)
        {
            Address = address;
            Name = name;
            NickName = nickname;
            Email = email;
            Score = score;
            Type = PlayerTypes.ChessPlayer;
        }

        public FigureTypes FigureType { get; set; }
    }
}
