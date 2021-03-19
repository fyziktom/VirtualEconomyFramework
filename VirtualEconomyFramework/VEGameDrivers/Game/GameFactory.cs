using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEGameDrivers.Game
{
    public static class GameFactory
    {
        public static IGame GetGame(GameTypes type, string id, string player1, string player2, string actualStateTx)
        {
            switch (type)
            {
                case GameTypes.ChessGame:
                    return new ChessGame(id, player1, player2, actualStateTx);
            }

            return null;
        }
    }
}
