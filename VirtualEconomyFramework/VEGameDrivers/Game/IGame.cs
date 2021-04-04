using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEGameDrivers.Game.GameDto;
using VEGameDrivers.Players;

namespace VEGameDrivers.Game
{
    public enum GameTypes
    {
        BasicGame,
        ChessGame
    }
    public interface IGame
    {
        GameTypes Type { get; set; }
        Guid Id { get; set; }
        string ActualStateTxId { get; set; }
        List<string> GameHistoryTxIds { get; set; }
        List<IGameDto> GameHistory { get; set; }
        bool Finished { get; set; }
        string Winner { get; set; }
        IDictionary<string, IPlayer> Players { get; set; }
        IDictionary<string, Bet> Bets { get; set; }

        Task<string> LoadHistory();
        Task<string> LoadPlayers();
        Task<string> SendNewGameRequest(GameTypes type, string address, string password = "");
        Task<string> SendCapitulationRequest(GameTypes type, string address, string password = "");
    }
}
