using System;
using System.Collections.Generic;
using System.Text;

namespace VEGameDrivers.Players
{
    public static class PlayersFactory
    {
        public static IPlayer GetPlayer(PlayerTypes type, string address, string name, string nickname, string email, double score)
        {
            switch (type)
            {
                case PlayerTypes.BasicPlayer:
                    return new BasicPlayer(address, name, nickname, email, score);
                case PlayerTypes.ChessPlayer:
                    return new ChessPlayer(address, name, nickname, email, score);
            }

            return null;
        }
    }
}
