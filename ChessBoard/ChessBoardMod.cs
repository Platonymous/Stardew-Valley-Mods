using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PyTK;
using PyTK.CustomElementHandler;
using PyTK.Extensions;
using PyTK.Types;
using StardewModdingAPI;
using StardewValley;
using System.Collections.Generic;
using System.IO;

namespace ChessBoard
{
    public class ChessBoardMod : Mod
    {
        internal static IMonitor monitor;
        internal ITranslationHelper i18n => Helper.Translation;
        internal static Config config;

        internal static string fullReceiverName = "Platonymous.Chess.Full";
        internal static string sessionReceiverName = "Platonymous.Chess.Sessions";
        internal static string historyReciverName = "Platonymous.Chess.History";
        

        PyReceiver<Session> SessionReceiver;
        PyReceiver<GameResult> HistoryReceiver;
        PyReceiver<SaveData> FullReceiver;

        public override void Entry(IModHelper helper)
        {
            monitor = Monitor;
            config = helper.ReadConfig<Config>();
            ChessGame.LoadTextures(helper);
            helper.ConsoleCommands.Add("chess", "[chess {id} (open)] starts or laods a chess session", (s, p) =>
            {
                if (Context.IsWorldReady)
                    Game1.currentMinigame = new ChessGame(p.Length == 0 ? "ConsoleGame" : p[0], p.Length == 0 || (p.Length > 1 && p[1] == "open"), helper);
                else
                    Monitor.Log("You need to load a save first.", LogLevel.Error);
            });

            TileAction Lock = new TileAction("StartChess", (a,l,v,s) =>
            {
                var p = a.Split(' ');
                Game1.currentMinigame = new ChessGame(p.Length < 1 ? "ConsoleGame" : p[1], p.Length == 0 || (p.Length > 2 && p[2] == "open"), helper);
                return true;
            }).register();

            FullReceiver = new PyReceiver<SaveData>(fullReceiverName, (s) =>
            {
                monitor.Log("Receiving Data");

                ChessGame.SavedGameData = s;
            }, 30, SerializationType.JSON);

            SessionReceiver = new PyReceiver<Session>(sessionReceiverName, (s) =>
              {
                  monitor.Log("Receiving Session: " + s.Id);

                  ChessGame.SavedGameData.Sessions.AddOrReplace(s.Id, s);

                  if (Game1.currentMinigame is ChessGame cg && cg.Id == s.Id)
                      cg.updateSession();

              }, 30, SerializationType.JSON);

            HistoryReceiver = new PyReceiver<GameResult>(historyReciverName, (s) =>
              {
                  monitor.Log("Receiving Result: " + s.Winner);

                  LogGameResult(s);
              }, 30, SerializationType.JSON);

            FullReceiver.start();
            SessionReceiver.start();
            HistoryReceiver.start();

            helper.Events.Multiplayer.PeerContextReceived += (s, e) =>
            {
                if (Game1.IsMasterGame)
                {
                    monitor.Log("Sending Data");
                    PyNet.sendDataToFarmer(fullReceiverName, ChessGame.SavedGameData, e.Peer.PlayerID, SerializationType.JSON);
                }
            };

            helper.Events.GameLoop.SaveLoaded += (s, e) =>
            {
                if (Game1.IsMasterGame && helper.Data.ReadSaveData<SaveData>("Platonymous.Chess") is SaveData sav)
                    ChessGame.SavedGameData = sav;
            };

            helper.Events.GameLoop.Saving += (s, e) =>
            {
                if (Game1.IsMasterGame)
                    helper.Data.WriteSaveData("Platonymous.Chess", ChessGame.SavedGameData);
            };

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            if (Helper.ModRegistry.GetApi<IMobilePhoneApi>("aedenthorn.MobilePhone") is IMobilePhoneApi api)
            {
                Texture2D appIcon = Helper.Content.Load<Texture2D>(Path.Combine("assets", "mobile_app_icon.png"));
                bool success = api.AddApp(Helper.ModRegistry.ModID + "MobileChess", "Chess", () =>
                {
                    Game1.currentMinigame = new ChessGame("Mobile", true, Helper);
                }, appIcon);
            }
        }

        public static void SyncSession(Session session)
        {
            monitor.Log("Sending Session: " + session.Id);
            ChessGame.SavedGameData.Sessions.AddOrReplace(session.Id, session);
            PyNet.sendRequestToAllFarmers<bool>(sessionReceiverName, session, null, SerializationType.JSON, -1);
        }

        public static void SyncGameResult(GameResult result)
        {
            monitor.Log("Sending Result: " + result.Winner);

            LogGameResult(result);
            PyNet.sendRequestToAllFarmers<bool>(historyReciverName, result, null, SerializationType.JSON, -1);
        }

        public static void LogGameResult(GameResult s)
        {
            ChessGame.SavedGameData.LastSessions.Add(s);

            if (ChessGame.SavedGameData.LastSessions.Count > 10)
                ChessGame.SavedGameData.LastSessions.Remove(ChessGame.SavedGameData.LastSessions[0]);

            string loser = s.WhitePlayer == s.Winner ? s.BlackPlayer : s.WhitePlayer;

            if (!ChessGame.SavedGameData.LeaderBoard.ContainsKey(s.Winner))
                ChessGame.SavedGameData.LeaderBoard.Add(s.Winner, new LeaderBoardEntry(s.Winner,true));
            else
            {
                ChessGame.SavedGameData.LeaderBoard[s.Winner].Games++;
                ChessGame.SavedGameData.LeaderBoard[s.Winner].Wins++;
            }

            if (!ChessGame.SavedGameData.LeaderBoard.ContainsKey(loser))
                ChessGame.SavedGameData.LeaderBoard.Add(loser, new LeaderBoardEntry(loser,false));
            else
            {
                ChessGame.SavedGameData.LeaderBoard[loser].Games++;
                ChessGame.SavedGameData.LeaderBoard[loser].Losses++;
            }
        }

    }
}
