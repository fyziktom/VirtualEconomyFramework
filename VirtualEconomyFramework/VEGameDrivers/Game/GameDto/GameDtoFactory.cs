using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEGameDrivers.Players;

namespace VEGameDrivers.Game.GameDto
{
    public static class GameDtoFactory
    {
        public static IGameDto GetGameDto(GameDtoTypes type, string id, string lastStateTxId)
        {
            switch (type)
            {
                case GameDtoTypes.BasicGame:
                    return new BasicGameDto(id, lastStateTxId);
                case GameDtoTypes.ChessGame:
                    return new ChessGameDto(id, lastStateTxId);
            }

            return null;
        }
    }
}
