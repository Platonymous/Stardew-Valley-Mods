using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using LandGrants.Game;

namespace LandGrants
{
    public class WerwolfMod : Mod
    {
        private Config Config;
        private Harmony HarmonyInstance;

        public override void Entry(IModHelper helper)
        {
            HarmonyInstance = new Harmony("Platonymous.Werewolf");
            helper.Events.Multiplayer.ModMessageReceived += Multiplayer_ModMessageReceived;
            helper.Events.Multiplayer.PeerDisconnected += Multiplayer_PeerDisconnected;
            helper.Events.GameLoop.OneSecondUpdateTicked += GameLoop_OneSecondUpdateTicked;
            helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
            helper.Events.GameLoop.ReturnedToTitle += GameLoop_ReturnedToTitle;
            helper.Events.Display.MenuChanged += Display_MenuChanged;
            helper.Events.Input.ButtonReleased += Input_ButtonReleased;
            WerwolfGame.LoadBasicRoles();
            Config = Helper.ReadConfig<Config>();

            helper.ConsoleCommands.Add("Werewolf", "Starts a game of Werewolf", (s, p) =>
            {
                Config = helper.ReadConfig<Config>();
                if (Context.IsMainPlayer)
                    StartWerwolf(Monitor, Helper, Config);
                else
                    Monitor.Log("Only the host can start a Werewolf game.", LogLevel.Error);
            });

            HarmonyInstance.Patch(AccessTools.DeclaredMethod(typeof(NPC), "draw"), new HarmonyMethod(this.GetType(), nameof(DrawPrefix)));
        }

        private void Display_MenuChanged(object sender, StardewModdingAPI.Events.MenuChangedEventArgs e)
        {
            if (Context.IsGameLaunched && Context.IsMainPlayer 
                && e.NewMenu is DialogueBox db 
                && db?.dialogues?.FirstOrDefault() is string text
                && text == Game1.content.LoadString("Strings\\StringsFromMaps:Town.3")
                && WerwolfGame.CurrentGame == null)
            {
                ShowWerwolfStartQuestion();
            }
        }

        private void ShowWerwolfStartQuestion()
        {
            if (Game1.activeClickableMenu == null)
            {

                var oldAfterQ = Game1.currentLocation.afterQuestion;
                Game1.activeClickableMenu = new DialogueBox("???: Do you want to play a game?", new Response[] { new Response("yes", "Yes"), new Response("no", "No") });
                Game1.currentLocation.afterQuestion = (who, answer) =>
                {

                    if (answer == "yes")
                    {
                        Config = Helper.ReadConfig<Config>();
                        StartWerwolf(Monitor, Helper, Config);
                    }

                    Game1.currentLocation.afterQuestion = oldAfterQ;

                };
            }
            else
                Game1.delayedActions.Add(new DelayedAction(500, () => ShowWerwolfStartQuestion()));
        }

        private void GameLoop_ReturnedToTitle(object sender, StardewModdingAPI.Events.ReturnedToTitleEventArgs e)
        {
            WerwolfGame.CurrentGame = null;
            WerwolfClientGame.CurrentGame = null;
            WerwolfGame.BotGames.Clear();
        }

        private void GameLoop_DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            WerwolfGame.CurrentGame?.DayStart();
        }

        private void GameLoop_UpdateTicked(object sender, StardewModdingAPI.Events.UpdateTickedEventArgs e)
        {
            if (!e.IsMultipleOf(20) || !Context.IsWorldReady)
                return;

            if (WerwolfClientGame.CurrentGame is WerwolfClientGame wcg && wcg.LocalPlayer.Alive)
            {
                var actions = GetAvailableActions(wcg);
                if (actions.Count > 0)
                {
                    NPC npc = null;
                    Farmer farmer = null;
                    if (Game1.currentLocation.characters.Where(c => c.Name != wcg.LocalPlayer.Charakter && wcg.Players.Any(p => p.Alive && p.Charakter == c.Name))
                    .ToList().OrderBy(npc => GetSquaredDistance(npc.Position, Game1.player.Position))
                    .ToList() is List<NPC> npcs && npcs.Count > 0)
                        npc = npcs.FirstOrDefault(n => GetSquaredDistance(n.Position, Game1.player.Position) < 30000);

                    if (Game1.currentLocation.farmers.Where(fa => fa.UniqueMultiplayerID != wcg.LocalPlayer.ID && wcg.Players.Any(p => p.Alive && p.ID == fa.UniqueMultiplayerID)).ToList()
                        .OrderBy(f => GetSquaredDistance(f.Position, Game1.player.Position))
                    .ToList() is List<Farmer> farmers && farmers.Count > 0)
                        farmer = farmers.FirstOrDefault(n => GetSquaredDistance(n.Position, Game1.player.Position) < 30000);

                    if (npc is null && farmer is null)
                        return;

                    if (npc is not null && farmer is null)
                    {
                        if (!npc.IsEmoting)
                        {
                            npc.doEmote(12, false, false);
                            Game1.delayedActions.Add(new DelayedAction(500, () => npc.IsEmoting = false));
                        }
                    }
                    else if (npc is null && farmer is not null)
                    {
                        if (!farmer.IsEmoting)
                        {
                            farmer.doEmote(12, false, false);
                            Game1.delayedActions.Add(new DelayedAction(500, () => farmer.IsEmoting = false));
                        }
                    }
                    else if (npc is not null && farmer is not null)
                        if (GetSquaredDistance(npc.Position, Game1.player.Position) <= GetSquaredDistance(farmer.Position, Game1.player.Position))
                        {
                            if (!npc.IsEmoting)
                            {
                                npc.doEmote(12, false, false);
                                Game1.delayedActions.Add(new DelayedAction(500, () => npc.IsEmoting = false));
                            }
                        }
                        else
                        {
                            if (!farmer.IsEmoting)
                            {
                                farmer.doEmote(12, false, false);
                                Game1.delayedActions.Add(new DelayedAction(500, () => farmer.IsEmoting = false));
                            }
                        }
                }
            }

        }

        public static void DrawPrefix(NPC __instance, SpriteBatch b, ref float alpha)
        {
            if (WerwolfClientGame.CurrentGame is WerwolfClientGame wcg && wcg.Players.FirstOrDefault(p => p.Charakter == __instance.Name) is WerwolfClientPlayer wp && !wp.Alive)
                alpha = 0.5f;
        }
        
       private void AcceptActionAndSend(WerwolfClientGame wcg, string name, long target, Action action)
       {
           wcg.NextTask.Add(() =>
           {
               Game1.activeClickableMenu = new DialogueBox($"{name}: {(wcg.Players.FirstOrDefault(p => p.ID == target) is WerwolfClientPlayer wcp ? (wcp.Name + "/" + wcp.Charakter) : "???" )}", new Response[] { new Response("yes", "Yes"), new Response("no", "No") });
               var oldAfterQ = Game1.currentLocation.afterQuestion;
               Game1.currentLocation.afterQuestion = (who, answer) =>
               {
                   Game1.currentLocation.afterQuestion = oldAfterQ;
                   if (answer == "yes")
                       wcg.NextTask.Add(action);
               };
           });
       }
        
       private void CallActionOnTarget(WerwolfClientGame wcg, List<WerwolfClientAction> actions, long target)
       {
           if (target == 0)
               return;
           else if (actions.Count == 1)
               AcceptActionAndSend(wcg, actions.First().Name,target, () => wcg.SendAction(new WerwolfActionRequest(wcg, actions.First().ID, target)));
           else
               wcg.NextTask.Add(() =>
               {
                   Game1.activeClickableMenu = new DialogueBox("What do you want to do?", actions.Select(o => new Response($"{o.ID}", o.Name)).ToArray());
                   var oldAfterQ = Game1.currentLocation.afterQuestion;
                   Game1.currentLocation.afterQuestion = (who, answer) =>
                   {
                       Game1.currentLocation.afterQuestion = oldAfterQ;
                       AcceptActionAndSend(wcg, actions.First(a => a.ID == answer).Name,target, () => wcg.SendAction(new WerwolfActionRequest(wcg, answer, target)));
                   };
               });
       }
        
       private void CallAction(WerwolfClientGame wcg)
       {
            if (!wcg.LocalPlayer.Alive)
                return;

           var actions = GetAvailableActions(wcg);
           if (actions.Count > 0)
           {
               NPC npc = null;
               Farmer farmer = null;
               if (Game1.currentLocation.characters.Where(c => c.Name != wcg.LocalPlayer.Charakter && wcg.Players.Any(p => p.Alive && p.Charakter == c.Name))
               .ToList().OrderBy(npc => GetSquaredDistance(npc.Position, Game1.player.Position))
               .ToList() is List<NPC> npcs && npcs.Count > 0)
                   npc = npcs.FirstOrDefault(n => GetSquaredDistance(n.Position, Game1.player.Position) < 30000);

               if (Game1.currentLocation.farmers.Where(fa => fa.UniqueMultiplayerID != wcg.LocalPlayer.ID && wcg.Players.Any(p => p.Alive && p.ID == fa.UniqueMultiplayerID)).ToList()
                   .OrderBy(f => GetSquaredDistance(f.Position, Game1.player.Position))
               .ToList() is List<Farmer> farmers && farmers.Count > 0)
                   farmer = farmers.FirstOrDefault(n => GetSquaredDistance(n.Position, Game1.player.Position) < 30000);

               if (npc is null && farmer is null)
                   return;

                if (npc is not null && farmer is null)
                {
                    CallActionOnTarget(wcg, actions, wcg.Players.First(pp => pp.Charakter == npc.Name).ID);
                }
                else if (npc is null && farmer is not null)
                {
                    CallActionOnTarget(wcg, actions, farmer.UniqueMultiplayerID);
                }
                else if (npc is not null && farmer is not null)
                    if (GetSquaredDistance(npc.Position, Game1.player.Position) <= GetSquaredDistance(farmer.Position, Game1.player.Position))
                    {
                        CallActionOnTarget(wcg, actions, wcg.Players.First(pp => pp.Charakter == npc.Name).ID);
                    }
                    else
                    {
                        CallActionOnTarget(wcg, actions, farmer.UniqueMultiplayerID);
                    }
           }
       }
        
       private List<WerwolfClientAction> GetAvailableActions(WerwolfClientGame wcg)
       {
           if (wcg == null || !wcg.IsActive)
               return new List<WerwolfClientAction>();

           var actions = new List<WerwolfClientAction>();
           wcg.LocalPlayer.Roles.Where(r => r.Actions.Any(a => a.Active)).ToList().ForEach(role =>
           {
               actions.AddRange(role.Actions.Where(action => action.Active));
           });

           return actions;
       }
        
       private float GetSquaredDistance(Vector2 point1, Vector2 point2)
       {
           float a = (point1.X - point2.X);
           float b = (point1.Y - point2.Y);
           return (a * a) + (b * b);
       }

       private void Input_ButtonReleased(object sender, StardewModdingAPI.Events.ButtonReleasedEventArgs e)
       {
           if (WerwolfClientGame.CurrentGame is WerwolfClientGame wcg)
               if (e.Button == Config.ActionButton)
                   CallAction(wcg);
               else if(e.Button == Config.NotesButton)
                    if(WerwolfClientGame.CurrentGame is WerwolfClientGame w && w.InitialStatus)
                        WerwolfClientGame.CurrentGame?.ShowStatusLetter();
       }

        public static void StartWerwolf(IMonitor monitor, IModHelper helper, Config config)
        {
            if (config.FillWithBots || WerwolfGame.GetAllPeers(helper,config).Count >= config.MinPlayers)
                _ = new WerwolfGame(helper, monitor, Game1.player.UniqueMultiplayerID, config);
            else
                Game1.addHUDMessage(new HUDMessage("Could not start Werewolf: Not enough players!", 5000));
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            WerwolfGame.Fenris = new NPC(new AnimatedSprite("Characters\\Krobus", 0, 16, 24), new Vector2(31f, 17f) * 64f, "Sewer", 2, "Fenris", false, Helper.ModContent.Load<Texture2D>("assets/Fenris"));
        }

        private void GameLoop_OneSecondUpdateTicked(object sender, StardewModdingAPI.Events.OneSecondUpdateTickedEventArgs e)
        {
            WerwolfGame.CurrentGame?.ProcessCallbacks();
            WerwolfClientGame.CurrentGame?.ProcessTasks();
            WerwolfGame.BotGames.ForEach(b => b.ProcessTasks());
        }

        private void Multiplayer_PeerDisconnected(object sender, StardewModdingAPI.Events.PeerDisconnectedEventArgs e)
        {
            if(WerwolfGame.CurrentGame is WerwolfGame wg && wg.Players.Where(p => p.IsAlive && p.PlayerID == e.Peer.PlayerID) is WerwolfPlayer wp)
            {
                wp.HasDisconnected = true;
                wp.IsBot = true;
            }
        }

        private void Multiplayer_ModMessageReceived(object sender, StardewModdingAPI.Events.ModMessageReceivedEventArgs e)
        {
            if (Context.IsMainPlayer && e.Type == "werwolfsetname" && WerwolfGame.CurrentGame is WerwolfGame wg && wg.Players.FirstOrDefault(p => p.PlayerID == e.FromPlayerID) is WerwolfPlayer wp)
            {
                wp.Name = e.ReadAs<string>();
                wg.SendPlayerUpdateToAll();
            }
            else if (e.Type == "NewGame" && e.ReadAs<WerwolfNewGameRequest>() is WerwolfNewGameRequest newgame && newgame.SendTo == Game1.player.UniqueMultiplayerID)
            {
                Config = Helper.ReadConfig<Config>();
                ReceiveMPMessage("NewGame", newgame, Config, Monitor, Helper);
            }
            else
            {
                if (WerwolfClientGame.CurrentGame is WerwolfClientGame wcg)
                {
                    if (e.Type == "Message" && e.ReadAs<WerwolfMessage>() is WerwolfMessage message && message.SendTo == wcg.LocalPlayer.ID)
                        ReceiveMPMessage("Message", message, Config, Monitor, Helper);
                    else if (e.Type == "Choice" && e.ReadAs<WerwolfChoice>() is WerwolfChoice choice && choice.SendTo == wcg.LocalPlayer.ID)
                        ReceiveMPMessage("Choice", choice, Config, Monitor, Helper);
                    else if (e.Type == "Update" && e.ReadAs<WerwolfUpdate>() is WerwolfUpdate update && update.SendTo == wcg.LocalPlayer.ID)
                        ReceiveMPMessage("Update", update, Config, Monitor, Helper);
                }

                if (Context.IsMainPlayer && WerwolfGame.CurrentGame is WerwolfGame)
                {
                    if (e.Type == "Action" && e.ReadAs<WerwolfActionRequest>() is WerwolfActionRequest action)
                        ReceiveMPMessage("Action", action, Config, Monitor, Helper);
                    else if (e.Type == "Response" && e.ReadAs<WerwolfChoiceResponse>() is WerwolfChoiceResponse response)
                        ReceiveMPMessage("Response", response, Config, Monitor, Helper);
                }
            }
        }

        public static void ReceiveMPMessage(string type, WerwolfMPMessage mpmessage, Config config, IMonitor monitor, IModHelper helper)
        {
            if (type == "NewGame" && mpmessage is WerwolfNewGameRequest newgame)
                _ = new WerwolfClientGame(helper, monitor, newgame, Game1.player.UniqueMultiplayerID);
            else if (type == "Message" && mpmessage is WerwolfMessage message)
                WerwolfClientGame.CurrentGame?.ShowMessage(message);
            else if (type == "Choice" && mpmessage is WerwolfChoice choice)
                WerwolfClientGame.CurrentGame?.HandleChoice(choice);
            else if (type == "Update" && mpmessage is WerwolfUpdate update)
                WerwolfClientGame.CurrentGame?.HandleUpdate(update);
            else if (type == "Action" && mpmessage is WerwolfActionRequest action)
                WerwolfGame.CurrentGame?.ProcessActionRequest(action);
            else if (type == "Response" && mpmessage is WerwolfChoiceResponse response)
                WerwolfGame.CurrentGame?.ProcessCallbackAnswer(response);
        }
    }
}
