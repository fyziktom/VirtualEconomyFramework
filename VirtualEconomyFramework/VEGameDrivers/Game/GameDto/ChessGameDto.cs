using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEGameDrivers.Players;

namespace VEGameDrivers.Game.GameDto
{
    public class ChessGameDto : CommonGameDto
    {
        public ChessGameDto() 
        {
            Players = new Dictionary<string, ChessPlayer>();
        }
        public ChessGameDto(string id, string lastStateTx)
        {
            Type = GameDtoTypes.ChessGame;
            GameId = id;
            LastGameRecordTxId = lastStateTx;
        }
        public Dictionary<string, ChessPlayer> Players { get; set; }
        public string GameState { get; set; }
        public bool NewGameRequest { get; set; } = false;
        public bool CapitulationRequest { get; set; } = false;
    }
}
