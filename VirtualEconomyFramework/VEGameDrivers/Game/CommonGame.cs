using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEGameDrivers.Game.GameDto;
using VEGameDrivers.Players;

namespace VEGameDrivers.Game
{
    public abstract class CommonGame : IGame
    {
        public GameTypes Type { get; set; } = GameTypes.BasicGame;
        public Guid Id { get; set; } = Guid.Empty;
        public string ActualStateTxId { get; set; } = string.Empty;
        public List<string> GameHistoryTxIds { get; set; }
        public List<IGameDto> GameHistory { get; set; }
        public IDictionary<string, IPlayer> Players { get; set; }
        public IDictionary<string, Bet> Bets { get; set; }
        public bool Finished { get; set; }
        public string Winner { get; set; }

        public abstract Task<string> LoadHistory();
        public abstract Task<string> LoadPlayers();

        public abstract Task<string> SendCapitulationRequest(GameTypes type, string address, string password = "");

        public abstract Task<string> SendNewGameRequest(GameTypes type, string address, string password = "");
    }
}
