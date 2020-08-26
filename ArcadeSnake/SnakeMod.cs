using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PyTK;
using PyTK.CustomElementHandler;
using PyTK.Extensions;
using PyTK.Types;
using StardewModdingAPI;
using StardewValley;
using System.IO;

namespace Snake
{
    public class SnakeMod : Mod
    {
        public static IMonitor monitor;
        public static IModHelper helper;
        public static CustomObjectData sdata;
        public static string highscoreReceiverName = "Platonymous.Snake.HSReceiver";
        public static PyResponder<bool, Highscore> highscoreReceiver;

        public static string highscoreListReceiverName = "Platonymous.Snake.HSListReceiver";
        public static PyResponder<bool, HighscoreList> highscoreListReceiver;

        public override void Entry(IModHelper helper)
        {
            monitor = Monitor;
            SnakeMod.helper = helper;
            helper.Events.GameLoop.GameLaunched += (o, e) =>
            {
                sdata = new CustomObjectData("Snake", "Snake/0/-300/Crafting -9/Play 'Snake by Platonymous' at home!/true/true/0/Snake", helper.Content.Load<Texture2D>(@"Assets/arcade.png"), Color.White, bigCraftable: true, type: typeof(SnakeMachine));
            
                if (Helper.ModRegistry.GetApi<IMobilePhoneApi>("aedenthorn.MobilePhone") is IMobilePhoneApi api)
                {
                    Texture2D appIcon = Helper.Content.Load<Texture2D>(Path.Combine("assets", "mobile_app_icon.png"));
                    bool success = api.AddApp(Helper.ModRegistry.ModID + "MobileSnake", "Snake", () =>
                    {
                        Game1.currentMinigame = new SnakeMinigame(helper);
                    }, appIcon);
                }

            };
            helper.Events.GameLoop.SaveLoaded += (o, e) =>
            {
                if (Game1.IsMasterGame)
                {
                    SnakeMinigame.HighscoreTable = helper.Data.ReadSaveData<HighscoreList>("Platonymous.SnakeAcrcade.Highscore");
                    if (SnakeMinigame.HighscoreTable == null)
                        SnakeMinigame.HighscoreTable = new HighscoreList();

                    Monitor.Log("Loading Highscores");

                    foreach (Highscore h in SnakeMinigame.HighscoreTable.Entries)
                        Monitor.Log(h.Name + ": " + h.Value);
                }
                addToCatalogue();
            };


            helper.Events.GameLoop.Saving += (o, e) =>
            {
                if (Game1.IsMasterGame)
                    helper.Data.WriteSaveData<HighscoreList>("Platonymous.SnakeAcrcade.Highscore", SnakeMinigame.HighscoreTable);
            };

            highscoreReceiver = new PyResponder<bool, Highscore>(highscoreReceiverName, (score) =>
             {
                 if (Game1.IsMasterGame)
                 {
                     Monitor.Log("Received Highscore from " + score.Name + "(" + score.Value + ")");
                     SnakeMinigame.setScore(score.Name, score.Value);
                     PyNet.sendRequestToAllFarmers<bool>(highscoreListReceiverName, SnakeMinigame.HighscoreTable, null, serializationType: PyTK.Types.SerializationType.JSON);
                 }
                 return true;
             }, 60, requestSerialization: SerializationType.JSON);

            highscoreReceiver.start();

            highscoreListReceiver = new PyResponder<bool, HighscoreList>(highscoreListReceiverName, (score) =>
            {
                if (!Game1.IsMasterGame)
                {
                    Monitor.Log("Received Highscore Update");

                    foreach (Highscore h in score.Entries)
                        Monitor.Log(h.Name + ": " + h.Value);

                    SnakeMinigame.HighscoreTable = score;
                }
                return true;
            }, 40, requestSerialization: SerializationType.JSON);

            highscoreListReceiver.start();

            helper.Events.Multiplayer.PeerContextReceived += (s, e) =>
            {
                if(Game1.IsMasterGame)
                    PyUtils.setDelayedAction(5000, () => PyNet.sendRequestToAllFarmers<bool>(highscoreListReceiverName, SnakeMinigame.HighscoreTable, null, serializationType: PyTK.Types.SerializationType.JSON));
            };
        }

        public void addToCatalogue()
        {
            new InventoryItem(sdata.getObject(), 5000, 1).addToNPCShop("Gus");
        }
    }
}
