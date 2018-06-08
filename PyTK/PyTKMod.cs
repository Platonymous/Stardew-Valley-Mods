using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using SObject = StardewValley.Object;
using StardewValley.TerrainFeatures;
using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;
using PyTK.Extensions;
using PyTK.Types;
using PyTK.CustomElementHandler;
using PyTK.ConsoleCommands;
using PyTK.CustomTV;
using Harmony;
using System.Reflection;
using StardewValley.Menus;
using System.Collections.Generic;
using PyTK.Overrides;
using xTile.Format;
using System.Linq;
using PyTK.Tiled;
using PyTK.Lua;
using static PyTK.Overrides.OvSpritebatch;

namespace PyTK
{

    internal class Config
    {
        bool patchSpriteBatch { get; set; } = true;
    }

    public class PyTKMod : Mod
    {
        internal static IModHelper _helper;
        internal static IMonitor _monitor;
        internal static bool _activeSpriteBatchFix = true;
        internal static string sdvContentFolder => PyUtils.getContentFolder();
        internal static List<IPyResponder> responders;

        public override void Entry(IModHelper helper)
        {
            _helper = helper;
            _monitor = Monitor;

            //testing();
            //messageTest()

            harmonyFix();
            FormatManager.Instance.RegisterMapFormat(new NewTiledTmxFormat());

            SaveHandler.BeforeRebuilding += (a, b) => CustomObjectData.collection.useAll(k => k.Value.sdvId = k.Value.getNewSDVId());
            initializeResponders();
            startResponder();
            registerConsoleCommands();
            CustomTVMod.load();
            PyLua.init();
            SaveHandler.setUpEventHandlers();
            ContentSync.ContentSyncHandler.initialize();
        }

        private void harmonyFix()
        {
            HarmonyInstance instance = HarmonyInstance.Create("Platonymous.PyTK");
            PyUtils.initOverride("SObject", PyUtils.getTypeSDV("Object"),typeof(DrawFix1), new List<string>() { "draw", "drawInMenu", "drawWhenHeld", "drawAsProp" });
            PyUtils.initOverride("TemporaryAnimatedSprite", PyUtils.getTypeSDV("TemporaryAnimatedSprite"),typeof(DrawFix2), new List<string>() { "draw" });
            instance.PatchAll(Assembly.GetExecutingAssembly());
        }

        private void startResponder()
        {
            responders.ForEach(r => r.start());
        }

        private void stopResponder()
        {
            responders.ForEach(r => r.stop());
        }

        private void initializeResponders()
        {
            responders = new List<IPyResponder>();
            responders.Add(new PyResponder<int, int>("PytK.StaminaRequest", (s) =>
            {
                if (Game1.player == null)
                    return -1;

                if (s == -1)
                    return (int)Game1.player.Stamina;
                else
                {
                    Game1.player.Stamina = s;
                    return s;
                }

            }, 8));

            responders.Add(new PyResponder<bool, long>("PytK.Ping", (s) =>
            {
                return true;

            }, 1));

            responders.Add(new PyResponder<bool, WarpRequest>("PyTK.WarpFarmer", (w) =>
             {
                 try
                 {
                     Game1.warpFarmer(Game1.getLocationRequest(w.locationName, w.isStructure), w.x, w.y, w.facing < 0 ? Game1.player.FacingDirection : w.facing);
                     return true;
                 }
                 catch
                 {
                     return false;
                 }
             },16,SerializationType.PLAIN,SerializationType.JSON));
        }

        private void registerConsoleCommands()
        {
            CcLocations.clearSpace().register();
            CcSaveHandler.cleanup().register();
            CcSaveHandler.savecheck().register();
            CcTime.skip().register();
            CcLua.runScript().register();

            new ConsoleCommand("send", "sends a message to all players: send [address] [message]", (s, p) =>
            {
                if (p.Length < 2)
                    Monitor.Log("Missing address or message.", LogLevel.Alert);
                else
                {
                    string address = p[0];
                    List<string> parts = new List<string>(p);
                    parts.Remove(p[0]);
                    string message = String.Join(" ", p);
                    PyNet.sendMessage(address, message);
                    Monitor.Log("OK", LogLevel.Info);
                }

            }).register();

            new ConsoleCommand("messages", "lists all new messages on a specified address: messages [address]", (s, p) =>
            {
                if (p.Length == 0)
                    Monitor.Log("Missing address", LogLevel.Alert);
                else
                {
                    List<MPMessage> messages = PyNet.getNewMessages(p[0]).ToList();
                    foreach (MPMessage msg in messages)
                        Monitor.Log($"From {msg.sender.Name} : {msg.message}", LogLevel.Info);

                    Monitor.Log("OK", LogLevel.Info);
                }

            }).register();

            new ConsoleCommand("getstamina", "lists the current stamina values of all players", (s, p) =>
            {
                Monitor.Log(Game1.player.Name + ": " + Game1.player.Stamina, LogLevel.Info);
                foreach (Farmer farmer in Game1.otherFarmers.Values)
                {
                    var getStamina = PyNet.sendRequestToFarmer<int>("PytK.StaminaRequest", -1, farmer);
                    getStamina.Wait();
                    Monitor.Log(farmer.Name + ": " + getStamina.Result, LogLevel.Info);
                }
            }).register();

            new ConsoleCommand("setstamina", "changes the stamina of all or a specific player. use: setstamina [playername or all] [stamina]", (s, p) =>
            {
                if (p.Length < 2)
                    Monitor.Log("Missing parameter", LogLevel.Alert);

                Monitor.Log(Game1.player.Name + ": " + Game1.player.Stamina, LogLevel.Info);
                Farmer farmer = null;
                try
                {
                    farmer = Game1.otherFarmers.Find(k => k.Value.Name.Equals(p[0])).Value;
                }
                catch
                {
                    
                }

                if(farmer == null)
                {
                    Monitor.Log("Couldn't find Farmer", LogLevel.Alert);
                    return;
                }

                int i = -1;
                int.TryParse(p[1], out i);

                var setStamina = PyNet.sendRequestToFarmer<int>("PytK.StaminaRequest", i, farmer);
                setStamina.Wait();
                Monitor.Log(farmer.Name + ": " + setStamina.Result, LogLevel.Info);
            }).register();


            new ConsoleCommand("ping", "pings all other players", (s, p) =>
            {
                foreach (Farmer farmer in Game1.otherFarmers.Values)
                {
                    long t = Game1.currentGameTime.TotalGameTime.Milliseconds;
                    var ping = PyNet.sendRequestToFarmer<bool>("PytK.Ping", t, farmer);
                    ping.Wait();

                    long r = Game1.currentGameTime.TotalGameTime.Milliseconds;
                    if (ping.Result)
                        Monitor.Log(farmer.Name + ": " + (r - t) + "ms", LogLevel.Info);
                    else
                        Monitor.Log(farmer.Name + ": No Answer", LogLevel.Error);
                }
            }).register();

            new ConsoleCommand("syncmap", "Syncs map of a specified location to all clients. Exp.: syncmap Farm, syncmap BusStop, syncmao Town", (s, p) =>
            {
                if (p.Length < 1)
                    Monitor.Log("No Location specified. ");

                PyNet.syncLocationMapToAll(p[0]);
            }).register();

            
        }

        private void messageTest()
        {
            PyNet.sendMessage("Platonymous.PyTK.Test", "TestMessage");
            TimeEvents.TimeOfDayChanged += (s, e) => 
            {
                foreach(MPMessage msg in PyNet.getNewMessages("Platonymous.PyTK.Test"))
                {
                    string message = (string) msg.message;
                    string sender = msg.sender.Name;
                    //Do Something;
                } 
            };
        }

        private void testing()
        {
            CustomObjectData.newBigObject("Platonymous.BigTest", Game1.bigCraftableSpriteSheet.clone().setSaturation(0), Color.Aquamarine, "Test Machine", "Test Description", 24, craftingData: new CraftingData("Test Machine"));
            CustomObjectData.newObject("Platonymous.Rubici", Game1.objectSpriteSheet.clone().setSaturation(0), Color.Yellow, "Rubici", "Rubici Test", 16, "Rubici", "Minerals -2", 50, -300);
            new CustomObjectData("Platonymous.Rubico" + Color.Red.ToString(), "Rubico/250/-300/Minerals -2/Rubico/A precious stone that is sought after for its rich color and beautiful fluster.", Game1.objectSpriteSheet.clone().setSaturation(0), Color.Red, 16);

            Keys.K.onPressed(() => Monitor.Log($"Played: {Game1.currentGameTime.TotalGameTime.Minutes} min"));
            ButtonClick.UseToolButton.onTerrainClick<Grass>(o => Monitor.Log($"Number of Weeds: {o.numberOfWeeds}", LogLevel.Info));
            new InventoryItem(new Chest(true), 100).addToNPCShop("Pierre");
            new ItemSelector<SObject>(p => p.name == "Chest").whenAddedToInventory(l => l.useAll(i => i.name = "Test"));
            Helper.Content.Load<Texture2D>($"Maps/MenuTiles", ContentSource.GameContent).setSaturation(0).injectAs($"Maps/MenuTiles");
            Game1.objectSpriteSheet.clone().setSaturation(0).injectTileInto($"Maps/springobjects", 74);
            Game1.objectSpriteSheet.clone().setSaturation(0).injectTileInto($"Maps/springobjects", new Range(129, 166), new Range(129, 166));

            Func<string, GameLocation, Vector2, string, bool> tileActionTest = (s, l, t, ly) =>
             {
                 List<string> strings = s.Split(' ').ToList();
                 strings.Remove(strings[0]);
                 Game1.activeClickableMenu = new DialogueBox(String.Join(" ", s));
                 return true;
             };

            Action mapMergeTest = delegate ()
            {
                "Beach".toLocation().Map.mergeInto("Town".toLocation().Map, new Vector2(60, 30), new Rectangle(15, 15, 20, 20)).injectAs(@"Maps/Town");
                "Town".toLocation().clearArea(new Rectangle(60, 30, 20, 20));
                "Town".toLocation().Map.addAction(new Vector2(18, 60), new TileAction("testaction", tileActionTest).register(),"Smells interesting");
            };

            SaveEvents.AfterLoad += (s, e) => mapMergeTest();
        }
    }
}
