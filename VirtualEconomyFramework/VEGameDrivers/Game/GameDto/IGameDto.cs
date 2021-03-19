using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEGameDrivers.Players;

namespace VEGameDrivers.Game.GameDto
{
    public enum GameDtoTypes
    {
        BasicGame,
        ChessGame
    }
    public interface IGameDto
    {
        GameDtoTypes Type { get; set; }
        string GameId { get; set; }
        string TxId { get; set; }
        string LastGameRecordTxId { get; set; }
        DateTime TimeStamp { get; set; }
        Dictionary<string, IPlayer> Players  { get; set; }
    }
}
