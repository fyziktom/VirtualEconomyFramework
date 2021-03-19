using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEGameDrivers.Players;

namespace VEGameDrivers.Game.GameDto
{
    public class CommonGameDto : IGameDto
    {
        public GameDtoTypes Type { get; set; } = GameDtoTypes.BasicGame;
        public string GameId { get; set; } = string.Empty;
        public string TxId { get; set; } = string.Empty;
        public string LastGameRecordTxId { get; set; } = string.Empty;
        public DateTime TimeStamp { get; set; }
        public Dictionary<string, IPlayer> Players { get; set; }
    }
}
