using log4net;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using VEconomy.Common;
using VEDrivers.Economy.Wallets.Handlers;
using VEGameDrivers.Common;
using VEGameDrivers.Game;
using VEGameDrivers.Game.Chess;
using VEGameDrivers.Game.GameDto;

namespace VEconomy.Controllers
{
    [Route("api")]
    [ApiController]
    public class ChessController : Controller
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private BasicAccountHandler accountHandler = new BasicAccountHandler();

        [HttpGet]
        [Route("GetChessHistory/{account}")]
        public object GetChessHistory(string account)
        {
            return null;
        }

        [HttpGet]
        [Route("GetChessGamesIds/{account}")]
        public List<string> GetChessGamesIds(string account)
        {
            var tokens = accountHandler.FindTokenByMetadata(account, "GameId");

            var ids = new List<string>();

            foreach(var t in tokens)
            {
                if (t.Value.Metadata.TryGetValue("GameId", out var id))
                {
                    if (!ids.Contains(id))
                        ids.Add(id);
                }
            }

            if (ids.Count > 0)
            {
                if (MainGameDataContext.Games == null)
                    MainGameDataContext.Games = new ConcurrentDictionary<string, IGame>();
            }

            return ids;
        }

       
        [HttpGet]
        [Route("GetActualChessState/{gameId}")]
        public GameState GetActualChessState(string gameId)
        {
            if (MainGameDataContext.Games.TryGetValue(gameId, out var game))
            {
                var state = (game as ChessGame).GetActualGameState();
                return state;
            }
            else
            {
                throw new HttpResponseException((HttpStatusCode)501, "Game not found!");
            }
        }


        public class ChessLoadHistoryData
        {
            public string gameId { get; set; }
            public string player1Address { get; set; }
            public string player2Address { get; set; }
        }

        [HttpPut]
        [Route("LoadChessHistory")]
        public async Task<List<ChessGameDto>> LoadChessHistory([FromBody] ChessLoadHistoryData data)
        {
            if (MainGameDataContext.Games.TryGetValue(data.gameId, out var game))
            {
                var state = await (game as ChessGame).LoadHistoryGetState();
                return (game as ChessGame).GameHistory;
                //return state;
            }
            else
            {
                var gm = GameFactory.GetGame(GameTypes.ChessGame, data.gameId, data.player1Address, data.player2Address, string.Empty);
                MainGameDataContext.Games.TryAdd(data.gameId, gm);
                var state = await (gm as ChessGame).LoadHistoryGetState();
                return (gm as ChessGame).GameHistory; 
            }
        }

        public class ChessStartData
        {
            public string player1Address { get; set; }
            public string player2Address { get; set; }
        }

        [HttpPut]
        [Route("StartNewChessGame")]
        public async Task<string> StartNewChessGame([FromBody] ChessStartData data)
        {
            try
            {
                if (string.IsNullOrEmpty(data.player1Address) && string.IsNullOrEmpty(data.player2Address))
                    throw new HttpResponseException((HttpStatusCode)501, "Cannot start game players cannot be empty!");

                var id = Guid.NewGuid().ToString();
                var game = GameFactory.GetGame(GameTypes.ChessGame, id, data.player1Address, data.player2Address, string.Empty);

                if (MainGameDataContext.Games == null)
                    MainGameDataContext.Games = new ConcurrentDictionary<string, IGame>();

                MainGameDataContext.Games.TryAdd(id, game);

                var res = await game.SendNewGameRequest(GameTypes.ChessGame, data.player2Address);

                return id.ToString();
            }
            catch (Exception ex)
            {
                log.Error("Cannot create new game", ex);
                throw new HttpResponseException((HttpStatusCode)501, "Cannot create new game!");
            }
        }

        public class ChessMoveData
        {
            public string accountAddress { get; set; }
            public string gameId { get; set; }
            public string chessboardState { get; set; }
        }

        [HttpPut]
        [Route("ConfirmChessMove")]
        public async Task<string> ConfirmChessMove([FromBody] ChessMoveData data)
        {
            try
            {
                if (string.IsNullOrEmpty(data.gameId))
                    throw new HttpResponseException((HttpStatusCode)501, "Cannot write move, wrong game Id!");

                if (string.IsNullOrEmpty(data.accountAddress))
                   throw new HttpResponseException((HttpStatusCode)501, "Cannot write move, wrong account address!");

                if (MainGameDataContext.Games.TryGetValue(data.gameId, out var game))
                {
                    var res = await (game as ChessGame).WriteMove(data.chessboardState, (game as ChessGame).Player2Address);
                    return res;
                }
                else
                {
                    throw new HttpResponseException((HttpStatusCode)501, "Cannot write move, game does not exists!");
                }
            }
            catch (Exception ex)
            {
                log.Error("Cannot create chess move", ex);
                throw new HttpResponseException((HttpStatusCode)501, "Cannot write chess move!");
            }
        }
    }


}
