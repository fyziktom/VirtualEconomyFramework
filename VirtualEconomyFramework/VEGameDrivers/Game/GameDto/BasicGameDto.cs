using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEGameDrivers.Players;

namespace VEGameDrivers.Game.GameDto
{
    public class BasicGameDto : CommonGameDto
    {
        public BasicGameDto(string id, string lastStateTx)
        {
            Type = GameDtoTypes.BasicGame;
            GameId = id;
            LastGameRecordTxId = lastStateTx;
        }
    }
}
