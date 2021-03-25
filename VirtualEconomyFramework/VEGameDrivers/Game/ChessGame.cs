using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VEDrivers.Economy.DTO;
using VEDrivers.Economy.Tokens;
using VEDrivers.Economy.Transactions;
using VEDrivers.Economy.Wallets.Handlers;
using VEGameDrivers.Game.Chess;
using VEGameDrivers.Game.GameDto;
using VEGameDrivers.Players;

namespace VEGameDrivers.Game
{
    public class ChessGame : CommonGame
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public ChessGame(string id, string player1, string player2, string actualStateTx)
        {
            Type = GameTypes.ChessGame;
            try
            {
                Id = new Guid(id);
            }
            catch(Exception ex)
            {
                log.Error("Chess Game - Wrong format of GUID. New one will be created!", ex);
                Id = Guid.NewGuid();
            }
            ActualStateTxId = actualStateTx;
            GameHistoryTxIds = new List<string>();
            GameHistory = new List<ChessGameDto>();
            Bets = new Dictionary<string, Bet>();
            Players = new Dictionary<string, ChessPlayer>();
            Player1Address = player1;
            Player2Address = player2;
            ChessPlayer pl1 = (ChessPlayer)PlayersFactory.GetPlayer(PlayerTypes.ChessPlayer, player1, string.Empty, string.Empty, string.Empty, 0.0);
            pl1.FigureType = FigureTypes.White;
            ChessPlayer pl2 = (ChessPlayer)PlayersFactory.GetPlayer(PlayerTypes.ChessPlayer, player2, string.Empty, string.Empty, string.Empty, 0.0);
            pl2.FigureType = FigureTypes.Black;
            Players.TryAdd(player1, pl1);
            Players.TryAdd(player2, pl2);
        }
        public Dictionary<string, ChessPlayer> Players { get; set; }
        public List<ChessGameDto> GameHistory { get; set; }
        public string Player1Address { get; set; } = string.Empty;
        public string Player2Address { get; set; } = string.Empty;
        private BasicAccountHandler accountHandler = new BasicAccountHandler();
        private string TokenSymbol = "CHESS";
        private string TokenId = "La46fzZotg6wgr2DncwfuakqadHanPWFTARqRL";

        public override async Task<string> LoadHistory()
        {
            return "OK";
        }

        public async Task<GameState> LoadHistoryGetState()
        {
            var tokens = accountHandler.FindTokenByMetadata(Player1Address, "GameId", Id.ToString());
            GameState lastState = null;
            GameHistory.Clear();
            GameHistoryTxIds.Clear();
            //var tok = tokens.Values.OrderBy(t => t.TimeStamp);

            foreach(var t in tokens)
            {
                try
                {
                    if (t.Value.Metadata != null)
                    {
                        if (t.Value.Metadata.TryGetValue("GameData", out var gameData))
                        {
                            var parsedData = JsonConvert.DeserializeObject<ChessGameDto>(gameData);

                            if (parsedData != null)
                            {
                                if (parsedData.GameId == Id.ToString() && parsedData.Type == GameDtoTypes.ChessGame)
                                {
                                    //GameHistoryTxIds.Add(t.Key);

                                    if (parsedData.Players.Count > 0)
                                        Players = parsedData.Players;

                                    parsedData.TxId = t.Key;
                                    parsedData.TimeStamp = t.Value.TimeStamp;
                                    GameHistory.Add(parsedData);
                                }

                                lastState = JsonConvert.DeserializeObject<GameState>(parsedData.GameState);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error("Chess Game - wrong format of gameData in token metadata. Cannot load history", ex);
                    return null;
                }
            }

            var ghist = GameHistory.OrderBy(t => t.TimeStamp);
            GameHistory = ghist.ToList<ChessGameDto>();
            foreach (var gh in GameHistory)
                GameHistoryTxIds.Add(gh.TxId);

            var last = GameHistoryTxIds.LastOrDefault();
            if (last != null)
                ActualStateTxId = last;

            return await Task.FromResult(lastState);
        }
        
        public async Task<string> LoadHistoryFromActualTx()
        {
            try
            {
                var allLoaded = false;
                var actualTxLoading = ActualStateTxId;
                while (!allLoaded)
                {
                    var info = await NeblioTransactionHelpers.TransactionInfoAsync(null, TransactionTypes.Neblio, actualTxLoading);

                    if (info == null)
                        return await Task.FromResult("OK - No History yet.");

                    if (info.VoutTokens != null)
                    {
                        if (info.VoutTokens.Count > 0)
                        {
                            var token = info.VoutTokens[0];
                            if (token.Metadata != null)
                            {
                                if (token.Metadata.Count > 0)
                                {
                                    if (token.Metadata.TryGetValue("GameId", out var gameid))
                                    {
                                        if (gameid == Id.ToString())
                                        {
                                            if (token.Metadata.TryGetValue("GameData", out var gameData))
                                            {
                                                if (!string.IsNullOrEmpty(gameData))
                                                {
                                                    try
                                                    {
                                                        var parsedData = JsonConvert.DeserializeObject<ChessGameDto>(gameData);
                                                        if (parsedData != null)
                                                        {
                                                            if (parsedData.GameId == Id.ToString() && parsedData.Type == GameDtoTypes.ChessGame)
                                                            {
                                                                GameHistoryTxIds.Add(actualTxLoading);

                                                                if (Players.Count == 0 && parsedData.Players.Count > 0)
                                                                    Players = parsedData.Players;

                                                                GameHistory.Add(parsedData);

                                                                if (parsedData.LastGameRecordTxId != "StartOfNewGame")
                                                                    actualTxLoading = parsedData.LastGameRecordTxId;
                                                                else
                                                                    allLoaded = true;
                                                            }
                                                        }
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        log.Error("Chess Game - wrong format of gameData in token metadata. Cannot load history", ex);
                                                        return null;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                // loading is going from newset to oldest one, so it is important to reverse whole list
                GameHistoryTxIds.Reverse();
                GameHistory.Reverse();

                return await Task.FromResult("OK");

            }
            catch(Exception ex)
            {
                log.Error("Chess Game - Cannot load game History!", ex);
                return await Task.FromResult("ERROR");
            }
        }
        

        public override async Task<string> LoadPlayers()
        {
            throw new NotImplementedException();
        }

        public override async Task<string> SendCapitulationRequest(GameTypes type, string address)
        {
            if (!Players.ContainsKey(address))
                return await Task.FromResult("Player address is not in the list of players!");

            try
            {
                var dto = new ChessGameDto()
                {
                    GameId = Id.ToString(),
                    GameState = string.Empty,
                    Type = GameDtoTypes.ChessGame,
                    LastGameRecordTxId = string.Empty,
                    CapitulationRequest = true,
                };

                dto.Players = new Dictionary<string, ChessPlayer>();

                if (Players.TryGetValue(Player1Address, out var pl1))
                    pl1.FigureType = FigureTypes.White;

                if (Players.TryGetValue(Player1Address, out var pl2))
                    pl2.FigureType = FigureTypes.Black;

                foreach (var pl in Players)
                    dto.Players.TryAdd(pl.Key, pl.Value);

                var moveString = JsonConvert.SerializeObject(dto);

                var tkData = new SendTokenTxData()
                {
                    Amount = 1,
                    Symbol = TokenSymbol,
                    SenderAddress = Player1Address,
                    ReceiverAddress = Player2Address,
                    Id = TokenId
                };

                tkData.Metadata.TryAdd("GameId", Id.ToString());
                tkData.Metadata.TryAdd("GameData", moveString);
                
                var res = await NeblioTransactionHelpers.SendNTP1TokenAPI(tkData, 30000);
                
                return await Task.FromResult(res);
            }
            catch (Exception ex)
            {
                log.Error("Chess Game - Cannot write move!", ex);
                return await Task.FromResult("ERROR");
            }
        }

        public override async Task<string> SendNewGameRequest(GameTypes type, string address)
        {
            if (!Players.ContainsKey(address))
                return await Task.FromResult("Player address is not in the list of players!");

            try
            {
                var dto = new ChessGameDto()
                {
                    GameId = Id.ToString(),
                    GameState = string.Empty,
                    Type = GameDtoTypes.ChessGame,
                    LastGameRecordTxId = string.Empty,
                    NewGameRequest = true,
                };

                dto.Players = new Dictionary<string, ChessPlayer>();

                foreach (var pl in Players)
                    dto.Players.TryAdd(pl.Key, pl.Value);

                var moveString = JsonConvert.SerializeObject(dto);
                
                var tkData = new SendTokenTxData()
                {
                    Amount = 1,
                    Symbol = TokenSymbol,
                    SenderAddress = Player1Address,
                    ReceiverAddress = Player2Address,
                    Id = TokenId
                };

                tkData.Metadata.TryAdd("GameId", Id.ToString());
                tkData.Metadata.TryAdd("GameData", moveString);
                
                var res = await NeblioTransactionHelpers.SendNTP1TokenAPI(tkData, 30000);

                if (!string.IsNullOrEmpty(res))
                    ActualStateTxId = res;

                return await Task.FromResult(res);
            }
            catch (Exception ex)
            {
                log.Error("Chess Game - Cannot write move!", ex);
                return await Task.FromResult("ERROR");
            }
        }

        public GameState GetActualGameState()
        {
            var st = GameHistory?.Last()?.GameState;
            if (!string.IsNullOrEmpty(st))
            {
                var state = JsonConvert.DeserializeObject<GameState>(st);
                return state;
            }
            else
            {
                return null;
            }
        }

        public async Task<string> WriteMove(string stateString, string onMoveAddress, string player2Address)
        {
            GameState state = null;

            if (!string.IsNullOrEmpty(stateString))
                state = GetGameStateFromString(stateString);
            else
                return await Task.FromResult("Cannot load state String. It is empty!");

            if (!Players.ContainsKey(onMoveAddress) || !Players.ContainsKey(player2Address))
                return await Task.FromResult("Player address is not in the list of players!");

            try
            {
                state.LastMovePlayer = onMoveAddress;
                var dto = new ChessGameDto()
                {
                    GameId = Id.ToString(),
                    GameState = JsonConvert.SerializeObject(state),
                    Type = GameDtoTypes.ChessGame,
                    LastGameRecordTxId = ActualStateTxId
                };

                dto.Players = new Dictionary<string, ChessPlayer>();

                foreach (var pl in Players)
                    dto.Players.TryAdd(pl.Key, pl.Value);

                GameHistory.Add(dto);

                var moveString = JsonConvert.SerializeObject(dto);
                
                var tkData = new SendTokenTxData()
                {
                    Amount = 1,
                    Symbol = TokenSymbol,
                    ReceiverAddress = player2Address,
                    SenderAddress = onMoveAddress,
                    Id = TokenId
                };

                tkData.Metadata.TryAdd("GameId", Id.ToString());
                tkData.Metadata.TryAdd("GameData", moveString);
                
                var res = await NeblioTransactionHelpers.SendNTP1TokenAPI(tkData, 30000);

                if (!string.IsNullOrEmpty(res))
                    ActualStateTxId = res;

                return await Task.FromResult(res);
            }
            catch(Exception ex)
            {
                log.Error("Chess Game - Cannot write move!", ex);
                return await Task.FromResult("ERROR");
            }

        }

        private GameState GetGameStateFromString(string inputArray)
        {
            var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(inputArray);
            var state = new GameState()
            {
                BoardState = data,
                LastMovePlayer = Player1Address,
                BoardStringData = inputArray
            };

            return state;
        } 
    }
}
