using StardewModdingAPI;
using StardewValley;
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
using xTile.Format;
using System.Linq;
using PyTK.Tiled;
using PyTK.Lua;
using xTile;
using xTile.Dimensions;
using PyTK.Overrides;
using PyTK.APIs;

namespace PyTK
{

    public class PyTKMod : Mod
    {
        internal static IModHelper _helper;
        internal static IModEvents _events => _helper.Events;
        internal static IMonitor _monitor;
        internal static bool _activeSpriteBatchFix = true;
        internal static string sdvContentFolder => PyUtils.ContentPath;
        internal static List<IPyResponder> responders;
        internal static PyTKSaveData saveData = new PyTKSaveData();

        public override void Entry(IModHelper helper)
        {
            _helper = helper;
            _monitor = Monitor;

            try
            {
                harmonyFix();
            }
            catch
            {
                Monitor.Log("Harmony Patching failed", LogLevel.Error);
            }

            FormatManager.Instance.RegisterMapFormat(new NewTiledTmxFormat());

            SaveHandler.BeforeRebuilding += (a, b) => CustomObjectData.collection.useAll(k => k.Value.sdvId = k.Value.getNewSDVId());
            initializeResponders();
            startResponder();
            registerConsoleCommands();
            CustomTVMod.load();
            PyLua.init();
            registerTileActions();
            SaveHandler.setUpEventHandlers();
            CustomObjectData.CODSyncer.start();
            ContentSync.ContentSyncHandler.initialize();
            this.Helper.Events.Player.Warped += Player_Warped;
            this.Helper.Events.GameLoop.DayStarted += OnDayStarted;
            this.Helper.Events.Multiplayer.PeerContextReceived += (s, e) =>
            {
                if (Game1.IsMasterGame && Game1.IsServer)
                {
                    if (CustomObjectData.collection.Values.Count > 0)
                    {
                        List<CODSync> list = new List<CODSync>();
                        foreach (CustomObjectData data in CustomObjectData.collection.Values)
                            list.Add(new CODSync(data.id, data.sdvId));

                        PyNet.sendDataToFarmer(CustomObjectData.CODSyncerName, new CODSyncMessage(list), e.Peer.PlayerID, SerializationType.JSON);
                    }

                    PyNet.sendDataToFarmer("PyTK.ModSavdDataReceiver", saveData, e.Peer.PlayerID, SerializationType.JSON);
                }
               
            };

            Helper.Events.Display.RenderingHud += (s, e) =>
            {
                if(Game1.displayHUD)
                    PyTK.PlatoUI.UIHelper.DrawHud(e.SpriteBatch, true);
            };

            Helper.Events.Display.RenderedHud += (s, e) =>
            {
                if (Game1.displayHUD)
                    PyTK.PlatoUI.UIHelper.DrawHud(e.SpriteBatch, false);
            };

            Helper.Events.Input.ButtonPressed += (s, e) =>
            {
                if (Game1.displayHUD)
                    if (e.Button == SButton.MouseLeft || e.Button == SButton.MouseRight)
                        PlatoUI.UIHelper.BaseHud.PerformClick(e.Cursor.ScreenPixels.toPoint(), e.Button == SButton.MouseRight, false, false);
            };

            Helper.Events.Display.WindowResized += (s, e) =>
            {
                PlatoUI.UIElement.Viewportbase.UpdateBounds();
                PlatoUI.UIHelper.BaseHud.UpdateBounds();
            };

            Helper.Events.Multiplayer.ModMessageReceived += PyNet.Multiplayer_ModMessageReceived;
            helper.Events.GameLoop.Saving += (s, e) =>
            {
                if(Game1.IsMasterGame)
                    try
                {
                    helper.Data.WriteSaveData<PyTKSaveData>("PyTK.ModSaveData",saveData);
                }
                catch
                {
                }
            };

            helper.Events.GameLoop.ReturnedToTitle += (s, e) =>
            {
                saveData = new PyTKSaveData();
            };

            helper.Events.GameLoop.SaveLoaded += (s, e) =>
            {
                if (Game1.IsMasterGame)
                {
                    try
                    {
                        saveData = helper.Data.ReadSaveData<PyTKSaveData>("PyTK.ModSaveData");
                    }
                    catch
                    {
                    }
                    if (saveData == null)
                        saveData = new PyTKSaveData();
                }
            };

            helper.Events.GameLoop.OneSecondUpdateTicked += (s, e) =>
            {
                if (Context.IsWorldReady && Game1.currentLocation is GameLocation location && location.Map is Map map)
                    PyUtils.checkDrawConditions(map);
            };
        }

        public static void syncCounter(string id, int value)
        {
            if (Game1.IsMultiplayer)
                PyNet.sendRequestToAllFarmers<bool>("PyTK.ModSavdDataCounterChangeReceiver", new ValueChangeRequest<string,int>(id,value,saveData.Counters[id]), null, SerializationType.JSON,-1);
        }

        public override object GetApi()
        {
            return new PyTKAPI();
        }

        private void Player_Warped(object sender, WarpedEventArgs e)
        {
            e.NewLocation?.Map.enableMoreMapLayers();

            if (e.NewLocation is GameLocation g && g.map is Map m)
            {
                if (m.Properties.ContainsKey("EntryAction"))
                    TileAction.invokeCustomTileActions("EntryAction", g, Vector2.Zero, "Map");

                PyUtils.checkDrawConditions(m);
            }
        }

        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            if (Game1.currentLocation is GameLocation g && g.map is Map m && m.Properties.ContainsKey("EntryAction"))
                TileAction.invokeCustomTileActions("EntryAction", g, Vector2.Zero, "Map");
        }

        private void harmonyFix()
        {
            HarmonyInstance instance = HarmonyInstance.Create("Platonymous.PyTK");
           // PyUtils.initOverride("SObject", PyUtils.getTypeSDV("Object"),typeof(DrawFix1), new List<string>() { "draw", "drawInMenu", "drawWhenHeld", "drawAsProp" });
           // PyUtils.initOverride("TemporaryAnimatedSprite", PyUtils.getTypeSDV("TemporaryAnimatedSprite"),typeof(DrawFix2), new List<string>() { "draw" });
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

            responders.Add(new PyReceiver<PyTKSaveData>("PyTK.ModSavdDataReceiver", (sd) =>
            {
                saveData.Counters = sd.Counters;
            }, 60, SerializationType.JSON));

            responders.Add(new PyReceiver<ValueChangeRequest<string,int>>("PyTK.ModSavdDataCounterChangeReceiver", (cr) =>
            {
                if (!saveData.Counters.ContainsKey(cr.Key))
                    saveData.Counters.Add(cr.Key, cr.Fallback);
                else
                    saveData.Counters[cr.Key] += cr.Value;
            }, 60, SerializationType.JSON));

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

        private void registerTileActions()
        {
            TileAction Game = new TileAction("Game", (action, location, tile, layer) =>
            {
                List<string> text = action.Split(' ').ToList();
                text.RemoveAt(0);
                action = String.Join(" ", text);
                return location.performAction(action, Game1.player, new Location((int)tile.X, (int)tile.Y));
            }).register();

            TileAction Lua = new TileAction("Lua", (action, location, tile, layer) =>
            {
                string[] a = action.Split(' ');
                if (a.Length > 2)
                    if (a[1] == "this")
                    {
                        string id = location.Name + "." + layer + "." + tile.Y + tile.Y;
                        if (!PyLua.hasScript(id))
                        {
                            if (layer == "Map")
                            {
                                if (location.map.Properties.ContainsKey("Lua_" + a[2]))
                                {
                                    string script = @"
                                function callthis(location,tile,layer)
                                " + location.map.Properties["Lua_" + a[2]].ToString() + @"
                                end";

                                    PyLua.loadScriptFromString(script, id);
                                }
                                else
                                    PyLua.loadScriptFromString("Luau.log(\"Error: Could not find Lua property on Map.\")", id);
                            }
                            else
                            {
                                if (location.doesTileHaveProperty((int)tile.X, (int)tile.Y, "Lua_" + a[2], layer) is string lua)
                                    PyLua.loadScriptFromString(@"
                                function callthis(location,tile,layer)
                                " + lua + @"
                                end", id);
                                else
                                    PyLua.loadScriptFromString("Luau.log(\"Error: Could not find Lua property on Tile.\")", id);
                            }
                        }
                        PyLua.callFunction(id, "callthis", new object[] { location, tile, layer });
                    }
                    else
                        PyLua.callFunction(a[1], a[2], new object[] { location, tile, layer });
                return true;
            }).register();

        }

        private void registerConsoleCommands()
        {
            CcLocations.clearSpace().register();
            CcSaveHandler.cleanup().register();
            CcSaveHandler.savecheck().register();
            CcTime.skip().register();
            CcLua.runScript().register();

            new ConsoleCommand("adjustWarps", "", (s, p) =>
            {
                PyUtils.adjustWarps(p[0]);

            }).register();

            new ConsoleCommand("rebuild_objects", "", (s, e) =>
              {
                  SaveHandler.RebuildAll(Game1.currentLocation.objects, Game1.currentLocation);
                  SaveHandler.RebuildAll(Game1.currentLocation.terrainFeatures, Game1.currentLocation);
              }).register();

            new ConsoleCommand("allready", "confirms all players for the current readydialogue", (s, p) =>
            {
                if (!(Game1.activeClickableMenu is ReadyCheckDialog))
                    Monitor.Log("No open ready check.", LogLevel.Alert);
                else
                    OvGame.allready = true;
            }).register();

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
                    PyNet.sendRequestToFarmer<int>("PytK.StaminaRequest", -1, farmer, (getStamina) => Monitor.Log(farmer.Name + ": " + getStamina, LogLevel.Info));
            }).register();

            new ConsoleCommand("setstamina", "changes the stamina of all or a specific player. use: setstamina [playername or all] [stamina]", (s, p) =>
            {
                if (p.Length < 2)
                    Monitor.Log("Missing parameter", LogLevel.Alert);

                Monitor.Log(Game1.player.Name + ": " + Game1.player.Stamina, LogLevel.Info);
                Farmer farmer = null;

                    farmer = Game1.otherFarmers.Find(k => k.Value.Name.Equals(p[0])).Value;


                if(farmer == null)
                {
                    Monitor.Log("Couldn't find Farmer", LogLevel.Alert);
                    return;
                }

                int i = -1;
                int.TryParse(p[1], out i);

                PyNet.sendRequestToFarmer<int>("PytK.StaminaRequest", i, farmer, (setStamina) => Monitor.Log(farmer.Name + ": " + setStamina, LogLevel.Info));
                
            }).register();


            new ConsoleCommand("ping", "pings all other players", (s, p) =>
            {
                foreach (Farmer farmer in Game1.otherFarmers.Values)
                {
                    long t = Game1.currentGameTime.TotalGameTime.Milliseconds;
                    PyNet.sendRequestToFarmer<bool>("PytK.Ping", t, farmer, (ping) =>
                    {
                        long r = Game1.currentGameTime.TotalGameTime.Milliseconds;
                        if (ping)
                            Monitor.Log(farmer.Name + ": " + (r - t) + "ms", LogLevel.Info);
                        else
                            Monitor.Log(farmer.Name + ": No Answer", LogLevel.Error);
                    });
                }
            }).register();

            new ConsoleCommand("syncmap", "Syncs map of a specified location to all clients. Exp.: syncmap Farm, syncmap BusStop, syncmao Town", (s, p) =>
            {
                if (p.Length < 1)
                    Monitor.Log("No Location specified. ");

                PyNet.syncLocationMapToAll(p[0]);
            }).register();
        }
    }
}
