using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using static StardewValley.GameLocation;

namespace LandGrants.Game
{
    public class WerwolfClientGame
    {
        public static WerwolfClientGame CurrentGame { get; set; } = null;

        public string GameID { get; set; }

        public long Host { get; set; }

        public List<WerwolfClientPlayer> Players { get; set; } = new List<WerwolfClientPlayer>();

        public bool IsActive { get; set; }

        public bool HasEnded { get; set; }

        public int Round { get; set; } = -1;

        public List<WerwolfClientPlayer> Winners { get; set; } = new List<WerwolfClientPlayer>();

        public string WinMessage { get; set; }

        public bool WolvesWon { get; set; } = false;

        public WerwolfClientPlayer LocalPlayer { get; set; }

        private IMonitor Monitor { get; set; }

        private IModHelper Helper { get; set; }

        private Config Config { get; set; }

        public bool InitialStatus { get; set; } = false;

        private Dictionary<string, LogLevel> LogLevelIndicators { get; } = new Dictionary<string, LogLevel>();

        private bool InfoScreenIsActive { get; set; } = false;

        public List<Action> NextTask { get; set; } = new List<Action>();

        private Random ClientRandom { get; set; }

        private afterQuestionBehavior OldAfterQ { get; set; }

        public string GameInfo { get; set; }

        public WerwolfClientGame()
        {

        }

        public WerwolfClientGame(IModHelper helper, IMonitor monitor, WerwolfNewGameRequest request, long player)
        {
            Helper = helper;
            Monitor = monitor;
            GameID = request.GameID;
            Host = request.Host;
            IsActive = true;
            Config = request.Config;
            LocalPlayer = request.InitialUpdate.Players.FirstOrDefault(p => p.ID == player);
            if(!LocalPlayer.IsBot)
                CurrentGame = this;
            ClientRandom = new Random();
            GameInfo = request.GameInfo;
            HandleUpdate(request.InitialUpdate);
            FillIndicators();
            Helper.Multiplayer.SendMessage(Game1.player.Name,"werwolfsetname", new[] { "Platonymous.Werewolf" }, new[] { Host });

            ShowBeginning();
        }

        private void FillIndicators()
        {
            LogLevelIndicators.Add("TRACE", LogLevel.Trace);
            LogLevelIndicators.Add("DEBUG", LogLevel.Debug);
            LogLevelIndicators.Add("WARN", LogLevel.Warn);
            LogLevelIndicators.Add("ALERT", LogLevel.Alert);
            LogLevelIndicators.Add("ERROR", LogLevel.Error);
            LogLevelIndicators.Add("INFO", LogLevel.Info);
        }

        private void SendMultiplayerMessage<T>(T message) where T : WerwolfMPMessage
        {
            if (message.SendTo == Game1.player.UniqueMultiplayerID || (Players.FirstOrDefault(p => p.ID == message.SendTo) is WerwolfClientPlayer wp && wp.IsBot))
                WerwolfMod.ReceiveMPMessage(message.Type, message, Config, Monitor, Helper);
            else
            {
                Helper.Multiplayer.SendMessage(message, message.Type, new[] { "Platonymous.Werewolf" }, new[] { message.SendTo });
            }
        }

        public void SendAction(WerwolfActionRequest action)
        {
            SendMultiplayerMessage(action);
        }

        public void SendResponse(WerwolfChoiceResponse response, bool resetHandler)
        {
            if (resetHandler)
                Game1.currentLocation.afterQuestion = OldAfterQ;

            SendMultiplayerMessage(response);
        }

        public void ShowInfo(WerwolfMessage message)
        {
            NextTask.Add(() => Game1.activeClickableMenu = new DialogueBox($"{message.Title}: {message.Message}"));
        }

        public void ShowBeginning()
        {
            if (LocalPlayer.IsBot)
                return;

            if (LocalPlayer.Roles.Any(r => r.Name == "Werewolf"))
                ShowDialog("Finally my reign can begin. You have been chosen to be my loyal servant. Every night you shall feed on the townsfolk so I may take their souls to grow my power.","Fenris");
            else
                ShowDialog("Oh no! Someone has released Fenris evil curse on the town. I can reverse it but first all those that have been turned into Werewolves have to be weeded out. Hold trials and get rid of those you can identify, the farmers each work with one of the residents. I will use all my power to protect the souls of those who die.", "Wizard");

            NextTask.Add(() =>
            {
                    Game1.delayedActions.Add(new DelayedAction(2000, () =>
                    {
                        if (!InitialStatus)
                            ShowStatusLetter();
                    }));
            });
        }

        public void ShowEnding()
        {
            if (!LocalPlayer.IsBot)
            {
                bool hasWon = Winners.Any(w => w.ID == LocalPlayer.ID);
                string win = hasWon ? "Congratulations!" : "You Lost!";

                Action last = () =>
                {
                    ShowDialog($"{win} {WinMessage}", WolvesWon ? "Fenris" : "Wizard");

                    ShowStatusLetter();

                    if (WolvesWon)
                        ShowDialog("Not so fast! No thanks to the others, I captured the remaining evildoers, break the curse and restore the village on my own. Go away, menace!", "Wizard");
                    else
                        ShowDialog("Fantastic! We managed to outsmart these foul creatures and now the curse is broken and I can restore the town. Try not to mess with dark forces again.", "Wizard");

                    ShowDialog("You got me this time Wizard, but this prison will not hold me forever.", "Fenris");
                    NextTask.Add(() => {
                    CurrentGame = null;
                    });
                };
                NextTask.Clear();
                NextTask.Add(last);
            }
            IsActive = false;
        }

        private void ShowDialog(string message, string npc)
        {
            NextTask.Add(() => Game1.activeClickableMenu = new DialogueBox(new Dialogue(npc == "Fenris" ? WerwolfGame.Fenris : Game1.getCharacterFromName(npc, true), "Werwolf.ShowDialogue", message )));
        }

        public bool TryParseColorFromString(string value, out Color color)
        {
            if (value == null || value.Length < 7 || !value.StartsWith("#"))
            {
                color = Color.White;
                return false;
            }

            if (value.Length == 7)
            {
                byte r = (byte)(Convert.ToUInt32(value.Substring(1, 2), 16));
                byte g = (byte)(Convert.ToUInt32(value.Substring(3, 2), 16));
                byte b = (byte)(Convert.ToUInt32(value.Substring(5, 2), 16));
                color = new Color() { R = r, G = g, B = b };
                return true;
            }
            else if (value.Length == 9)
            {
                byte a = (byte)(Convert.ToUInt32(value.Substring(1, 2), 16));
                byte r = (byte)(Convert.ToUInt32(value.Substring(3, 2), 16));
                byte g = (byte)(Convert.ToUInt32(value.Substring(5, 2), 16));
                byte b = (byte)(Convert.ToUInt32(value.Substring(7, 2), 16));

                color = new Color() { R = r, G = g, B = b, A = a };
                return true;
            }

            color = Color.White;
            return false;
        }

        public void ShowMessage(WerwolfMessage message)
        {
            if (message.SendTo == LocalPlayer.ID)
            {
                if (message.MessageType == WerwolfMessageType.DIALOG)
                    ShowDialog(message.Message, message.Title);

                if (message.MessageType == WerwolfMessageType.LOG)
                    Monitor.Log(message.Message, LogLevelIndicators[message.Title.ToUpper()]);

                if (message.MessageType == WerwolfMessageType.LETTER)
                    NextTask.Add(() => Game1.activeClickableMenu = new LetterViewerMenu(message.Message, message.Title));

                if (message.MessageType == WerwolfMessageType.INFO)
                    ShowInfo(message);
            }
        }

        public void ShowStatusLetter()
        {
            if (LocalPlayer.IsBot)
                return;

            if (Round >= 0 && !Players.Any(p => p.Name == "???"))
            {
                InitialStatus = true;

                var list = string.Join('^', Players.Select(p =>
                {
                    var roles = "???";
                    if (LocalPlayer.KnownRoles.TryGetValue(p.ID, out string r))
                        roles = r;
                    var isWolf = LocalPlayer.Roles.Any(r => r.Name == "Werewolf");
                    var alsoWolf = roles.Contains("Werewolf");
                    var indicator = p.ID == LocalPlayer.ID ? "+ " : "- ";

                    if (p.ID != LocalPlayer.ID && isWolf && alsoWolf)
                        indicator = "! ";

                    if (roles == "Unknown")
                        roles = "???";

                    if (!p.Alive)
                        indicator = "X ";
                    var pre = $"{indicator}{p.Name}/{p.Charakter}";
                    while (pre.Count() < 22)
                        pre = pre + " ";
                    var entry = $"{pre} {roles}";
                    return entry;
                }));
                var letter = $"Dear @, I made a list of what we know. -{LocalPlayer.Charakter}^{GameInfo}^ ^{list}^";
                NextTask.Add(() => Game1.activeClickableMenu = new LetterViewerMenu(letter, "Werewolf", false));
            }
            else
                NextTask.Add(() => Game1.delayedActions.Add(new DelayedAction(2000,() => ShowStatusLetter())));

        }

        public void ProcessTasks()
        {
            if(!LocalPlayer.IsBot && Game1.CurrentEvent is null && Game1.activeClickableMenu is null && NextTask.Count > 0)
            {
                var task = NextTask.First();
                NextTask.Remove(task);
                task();
            }

            if (NextTask.Count == 0 && !IsActive && CurrentGame == this)
            {
                CurrentGame = null;
            }
        }

        public void HandleUpdate(WerwolfUpdate update)
        {
            if (update.SendTo == LocalPlayer.ID)
            {
                Players = update.Players;
                Winners = update.Winners;
                WolvesWon = update.WolvesWon;
                HasEnded = Winners.Count > 0;
                WinMessage = update.WinMessage;
                Round = update.Round;
                LocalPlayer = Players.FirstOrDefault(p => p.ID == LocalPlayer.ID);
                if (HasEnded && IsActive)
                    ShowEnding();
            }
            else if (WerwolfGame.BotGames.FirstOrDefault(b => b.LocalPlayer.ID == update.SendTo) is WerwolfClientGame botGame)
                botGame.HandleUpdate(update);
        }

        public void HandleChoice(WerwolfChoice choice)
        {
            if (LocalPlayer.IsBot)
                SendResponse(new WerwolfChoiceResponse(this, choice.ChoiceID, choice.Options[(int)ClientRandom.Next(choice.Options.Count)].ID), false);
            else if (choice.SendTo == LocalPlayer.ID)
            {
                if (choice.ChoiceID.StartsWith("Vote_"))
                    ShowStatusLetter();

                NextTask.Add(() =>
                {
                    Game1.activeClickableMenu = new DialogueBox(choice.Question, choice.Options.Select(o => new Response($"{o.ID}", o.Name)).ToArray());
                    OldAfterQ = Game1.currentLocation.afterQuestion;
                    Game1.currentLocation.afterQuestion = (who, answer) => {

                        SendResponse(new WerwolfChoiceResponse(this, choice.ChoiceID, answer), true);


                        };
                });
            }
            else if (WerwolfGame.BotGames.FirstOrDefault(b => b.LocalPlayer.ID == choice.SendTo) is WerwolfClientGame botGame)
                botGame.HandleChoice(choice);


        }
    }
}
