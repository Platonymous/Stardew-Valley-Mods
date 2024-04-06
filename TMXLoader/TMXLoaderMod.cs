using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System.Collections.Generic;
using System.IO;
using xTile;
using HarmonyLib;
using System.Reflection;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Locations;
using System.Xml;
using xTile.ObjectModel;
using System;
using xTile.Tiles;
using xTile.Dimensions;
using StardewValley.TerrainFeatures;
using xTile.Layers;
using System.Collections;
using TMXLoader.Other;
using StardewValley.Monsters;
using System.Xml.Serialization;
using System.Threading.Tasks;
using StardewValley.Buildings;
using TMXTile;
using StardewValley.Network;
using StardewValley.Characters;
using StardewValley.Tools;
using StardewValley.Objects;
using StardewValley.GameData.Objects;
using StardewValley.GameData.Crops;
using Newtonsoft.Json.Linq;

namespace TMXLoader
{
    public class TMXLoaderMod : Mod
    {
        internal static IMonitor monitor;
        internal static IModHelper helper;
        internal static Dictionary<string, Map> mapsToSync = new Dictionary<string, Map>();
        internal static List<Farmer> syncedFarmers = new List<Farmer>();
        internal static Dictionary<TileShop, List<TileShopItem>> tileShops = new Dictionary<TileShop, List<TileShopItem>>();
        internal static Config config;
        internal static SaveData saveData = new SaveData();
        internal static List<MapEdit> addedLocations = new List<MapEdit>();
        internal static List<string> festivals = new List<string>();
        internal static List<TMXAssetEditor> conditionals = new List<TMXAssetEditor>();
        public static List<BuildableEdit> buildables = new List<BuildableEdit>();
        internal static List<SaveBuildable> buildablesBuild = new List<SaveBuildable>();
        internal static Dictionary<string, Warp> buildablesExits = new Dictionary<string, Warp>();
        internal static List<GameLocation> locationStorage = new List<GameLocation>();
        internal static Dictionary<string, ITranslationHelper> translators = new Dictionary<string, ITranslationHelper>();
        internal static List<TMXContentPack> latePacks = new List<TMXContentPack>(); 
        internal static TMXLoaderMod _instance;
        internal static Mod faInstance;
        internal static List<IPyResponder> responders;
        internal static PyTKSaveData pytksaveData = new PyTKSaveData();

        private readonly List<TMXAssetEditor> AssetEditors = new();

        /// <summary>The loaded buildable interior maps.</summary>
        public Dictionary<IAssetName, Map> BuildableInteriors = new();

        internal static bool contentPacksLoaded = false;


        public static List<IContentPack> AddedContentPacks = new List<IContentPack>();
        internal ITranslationHelper i18n => Helper.Translation;

        internal string buildableReceiverName = "BuildableReceiver";
        internal string buildableRemoverName = "BuildableRemover";

        internal PyReceiver<SaveBuildable> BuildableReceiver;
        internal PyReceiver<SaveBuildable> BuildableRemover;
        internal bool resetRain = false;
        internal bool resetRainValue = true;

        public override void Entry(IModHelper helper)
        {
            config = Helper.ReadConfig<Config>();
            config = Helper.ReadConfig<Config>();
            TMXLoaderMod.helper = Helper;
            monitor = Monitor;

            monitor?.Log("Open Buildables Menu with: " + config?.openBuildMenu, LogLevel.Info);

            _instance = this;
            var empty = new TileShop();
            empty.id = "EmptyShop";
            tileShops.Add(empty, new List<TileShopItem>());

            BuildableReceiver = new PyReceiver<SaveBuildable>(buildableReceiverName, (s) =>
            {
                if (Game1.IsMasterGame)
                    return;

                foreach (SaveBuildable b in buildablesBuild)
                    if (b.UniqueId == s.UniqueId)
                        return;

                Monitor?.Log("Received Placement for " + s.Id);
                loadSavedBuildable(s);
            }, 60, SerializationType.JSON);

            BuildableReceiver.start();

            BuildableRemover = new PyReceiver<SaveBuildable>(buildableRemoverName, (s) =>
            {
                if (Game1.IsMasterGame)
                    return;

                Monitor?.Log("Received Removal for " + s.Id);
                foreach (SaveBuildable b in buildablesBuild)
                    if (b.UniqueId == s.UniqueId)
                    {
                        removeSavedBuildable(b, false, false);
                        Monitor?.Log("Removed " + s.Id);

                        return;
                    }
            }, 60, SerializationType.JSON);

            BuildableRemover.start();


            helper.ConsoleCommands.Add("buildables", "Show Buildables Menu", (s, p) =>
            {
                showAll = true;
                if (Context.IsWorldReady && buildables.Count > 0)
                    showBuildablesMenu();
            });

            helper.Events.Content.AssetRequested += OnAssetRequested;

            helper.Events.GameLoop.ReturnedToTitle += (s, e) =>
            {
                removeAllSavedBuildables();
            };

            helper.Events.Input.ButtonReleased += (s, e) =>
            {
                showAll = false;
                if (e.Button == config.openBuildMenu.ToSButton() && Context.IsWorldReady && buildables.Count > 0)
                    showBuildablesMenu();
            };

            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            // helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;

            helper.Events.Multiplayer.PeerContextReceived += (s, e) =>
            {
                foreach (SaveBuildable b in buildablesBuild)
                    PyNet.sendDataToFarmer(buildableReceiverName, b, e.Peer.PlayerID, SerializationType.JSON);
            };


            helper.Events.Display.RenderingHud += (s, e) =>
            {
                PropertyValue rain = "";
                if (!(Game1.currentLocation is GameLocation location) || !(location.Map is Map map) || !map.Properties.TryGetValue("Raining", out rain))
                    return;

               

                if (Game1.isRaining && Game1.currentLocation.IsOutdoors && (!Game1.currentLocation.Name.Equals("Desert") && !(Game1.currentLocation is Summit)) && (!Game1.eventUp || Game1.currentLocation.isTileOnMap(new Vector2((float)(Game1.viewport.X / 64), (float)(Game1.viewport.Y / 64)))))
                    return;


                if (rain != "Never" && (Game1.isRaining || rain == "Always"))
                {
                    updateRainDrops();
                    for (int index = 0; index < Game1.rainDrops.Length; ++index)
                        Game1.spriteBatch.Draw(Game1.rainTexture, Game1.rainDrops[index].position, new Microsoft.Xna.Framework.Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.rainTexture, Game1.rainDrops[index].frame, -1, -1)), Color.White);
                }

            };

            helper.Events.Player.Warped += Player_Warped;

            helper.Events.GameLoop.SaveCreating += (s, e) => beforeSave();
            helper.Events.GameLoop.Saving += (s, e) => beforeSave();

            helper.Events.GameLoop.SaveCreated += (s, e) => afterSave();
            helper.Events.GameLoop.Saved += (s, e) => afterSave();
            helper.Events.Player.Warped += Player_WarpedPyTK;
            helper.Events.GameLoop.DayStarted += PYTKGameLoop_DayStarted;

            helper.Events.GameLoop.DayStarted += (s, e) =>
            {
                if(Game1.IsMasterGame)
                    foreach(GameLocation loc in Game1.locations)
                        setupCrops(loc);

                waterEverythingForRainProperty();

                if (!helper.ModRegistry.IsLoaded("Entoarox.FurnitureAnywhere"))
                    return;

                if (faInstance != null && faInstance.GetType() is Type Me && Me.GetMethod("WakeupFurniture", BindingFlags.NonPublic | BindingFlags.Instance) is MethodInfo wake)
                    foreach(var location in locationStorage)
                        wake.Invoke(faInstance, new[] { location });

                Game1.displayHUD = true;
            };

            helper.Events.GameLoop.SaveLoaded += (s, e) =>
            {
                Game1.displayHUD = true;
            };

            helper.Events.GameLoop.SaveCreated += (s, e) =>
            {
                Game1.displayHUD = true;
            };

            helper.Events.Display.MenuChanged += TMXActions.updateItemListAfterShop;

            PyTKEntry();
        }


        private void PyTKEntry()
        {
            Harmony instance = new Harmony("Platonymous.TMXLoader.Entry");
            instance.PatchAll(this.GetType().Assembly);

            helper.Events.Display.RenderingWorld += (s, e) =>
            {
                if (Game1.currentLocation is GameLocation location && location.Map is Map map && map.GetBackgroundColor() is TMXColor tmxColor)
                    Game1.graphics.GraphicsDevice.Clear(tmxColor.toColor());
            };
            
            initializeResponders();
            startResponder();
            PyLua.init();
            registerTileActions();
            registerEventPreconditions();
            bool adjustForCompat2 = false;
            bool hasMapTK = false;
            helper.Events.GameLoop.GameLaunched += (s, e) =>
            {

                if (xTile.Format.FormatManager.Instance.GetMapFormatByExtension("tmx") is TMXFormat tmxf)
                    tmxf.DrawImageLayer = PyMaps.drawImageLayer;

                hasMapTK = helper.ModRegistry.IsLoaded("Platonymous.MapTK");
                adjustForCompat2 = helper.ModRegistry.IsLoaded("DigitalCarbide.SpriteMaster");
                Game1.mapDisplayDevice = hasMapTK ? Game1.mapDisplayDevice : new PyDisplayDevice(Game1.content, Game1.graphics.GraphicsDevice, adjustForCompat2);
            };

            helper.Events.GameLoop.SaveLoaded += (s, e) =>
            {
                if (!(Game1.mapDisplayDevice is PyDisplayDevice || (Game1.mapDisplayDevice != null && Game1.mapDisplayDevice.GetType().Name.Contains("PyDisplayDevice"))))
                    Game1.mapDisplayDevice = hasMapTK ? Game1.mapDisplayDevice : PyDisplayDevice.Instance ?? new PyDisplayDevice(Game1.content, Game1.graphics.GraphicsDevice, adjustForCompat2);
            };

            helper.Events.GameLoop.SaveCreated += (s, e) =>
            {
                if (!(Game1.mapDisplayDevice is PyDisplayDevice || (Game1.mapDisplayDevice != null && Game1.mapDisplayDevice.GetType().Name.Contains("PyDisplayDevice"))))
                    Game1.mapDisplayDevice = hasMapTK ? Game1.mapDisplayDevice : PyDisplayDevice.Instance ?? new PyDisplayDevice(Game1.content, Game1.graphics.GraphicsDevice, adjustForCompat2);
            };

          
            helper.Events.GameLoop.ReturnedToTitle += (s, e) =>
            {
                foreach (Layer l in PyMaps.LayerHandlerList.Keys)
                {
                    foreach (var h in PyMaps.LayerHandlerList[l])
                    {
                        l.AfterDraw -= h;
                        l.BeforeDraw -= h;
                    }
                }

                PyMaps.LayerHandlerList.Clear();
            };

            this.Helper.Events.Multiplayer.PeerContextReceived += (s, e) =>
            {
                if (Game1.IsMasterGame && Game1.IsServer)
                {
                    PyNet.sendDataToFarmer("PyTK.ModSavdDataReceiver", saveData, e.Peer.PlayerID, SerializationType.JSON);
                }

            };

            Helper.Events.Display.RenderingHud += (s, e) =>
            {
                if (Game1.displayHUD && Context.IsWorldReady)
                    UIHelper.DrawHud(e.SpriteBatch, true);

            };

            Helper.Events.Display.RenderedHud += (s, e) =>
            {
                if (Game1.displayHUD && Context.IsWorldReady)
                    UIHelper.DrawHud(e.SpriteBatch, false);
            };

            Helper.Events.Input.ButtonPressed += (s, e) =>
            {
                if (Game1.displayHUD && Context.IsWorldReady)
                {
                    if (e.Button == SButton.MouseLeft || e.Button == SButton.MouseRight)
                        UIHelper.BaseHud.PerformClick(e.Cursor.ScreenPixels.toPoint(), e.Button == SButton.MouseRight, false, false);
                }
            };

            Helper.Events.Display.WindowResized += (s, e) =>
            {
                UIElement.Viewportbase.UpdateBounds();
                UIHelper.BaseHud.UpdateBounds();
            };

            Helper.Events.Multiplayer.ModMessageReceived += PyNet.Multiplayer_ModMessageReceived;
            helper.Events.GameLoop.Saving += (s, e) =>
            {
                if (Game1.IsMasterGame)
                    try
                    {
                        helper.Data.WriteSaveData<PyTKSaveData>("TMX.PyTK.ModSaveData", pytksaveData);
                    }
                    catch
                    {
                    }
            };

            helper.Events.GameLoop.ReturnedToTitle += (s, e) =>
            {
                pytksaveData = new PyTKSaveData();
            };

            helper.Events.GameLoop.SaveLoaded += (s, e) =>
            {
                if (Game1.IsMasterGame)
                {
                    try
                    {
                        pytksaveData = helper.Data.ReadSaveData<PyTKSaveData>("TMX.PyTK.ModSaveData");
                    }
                    catch
                    {
                    }
                    if (pytksaveData == null)
                        pytksaveData = new PyTKSaveData();
                }
            };

            helper.Events.GameLoop.OneSecondUpdateTicked += (s, e) =>
            {
                if (Context.IsWorldReady && Game1.currentLocation is GameLocation location && location.Map is Map map)
                    PyUtils.checkDrawConditions(map);
            };

            helper.Events.GameLoop.UpdateTicked += (s, e) => AnimatedTexture2D.ticked = e.Ticks;
        }

        private void registerTileActions()
        {
            TileAction CC = new TileAction("CC", (action, location, tile, layer) =>
            {
                List<string> text = action.Split(' ').ToList();
                string key = text[1];
                text.RemoveAt(0);
                text.RemoveAt(0);
                action = String.Join(" ", text);
                if (key == "cs")
                    action += ";";

                ICommandHelper commandHelper = Helper.ConsoleCommands;
                object commandManager = commandHelper.GetType().GetField("CommandManager", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)?.GetValue(commandHelper);
                if (commandManager is null)
                    throw new InvalidOperationException("Can't get SMAPI's underlying command manager.");

                MethodInfo triggerCommand = commandManager.GetType().GetMethod("Trigger", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (triggerCommand is null)
                    throw new InvalidOperationException("Can't get SMAPI's underlying CommandManager.Trigger method.");

                triggerCommand.Invoke(commandManager, new object[] { key, action.Split(' ') });
                return true;
            }).register();

            TileAction Game = new TileAction("Game", (action, location, tile, layer) =>
            {
                List<string> text = action.Split(' ').ToList();
                text.RemoveAt(0);
                action = String.Join(" ", text);
                return location.performAction(action, Game1.player, new xTile.Dimensions.Location((int)tile.X, (int)tile.Y));
            }).register();

            TileAction Lua = new TileAction("Lua", (action, location, tile, layer) =>
            {
                string[] a = action.Split(' ');
                if (a.Length > 2)
                    if (a[1] == "this")
                    {
                        string id = location.Name + "." + layer + "." + tile.X + "." + tile.Y + "." + a[2];
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
                            }
                            else
                            {
                                if (location.doesTileHaveProperty((int)tile.X, (int)tile.Y, "Lua_" + a[2], layer) is string lua)
                                    PyLua.loadScriptFromString(@"
                                function callthis(location,tile,layer)
                                " + lua + @"
                                end", id);
                            }
                        }

                        if (PyLua.hasScript(id))
                            PyLua.callFunction(id, "callthis", new object[] { location, tile, layer });
                    }
                    else
                    {
                        try
                        {
                            PyLua.callFunction(a[1], a[2], new object[] { location, tile, layer });
                        }
                        catch
                        {

                        }
                    }
                return true;
            }).register();

        }

        private void registerEventPreconditions()
        {
            PyUtils.addEventPrecondition("hasmod", (key, values, location) =>
            {
                string mod = values.Replace("hasmod ", "").Replace(" ", "");
                bool result = LuaUtils.hasMod(mod);
                return result;
            });

            PyUtils.addEventPrecondition("switch", (key, values, location) =>
            {
                return LuaUtils.switches(values.Replace("switch ", ""));
            });

            PyUtils.addEventPrecondition("npcxy", (key, values, location) =>
            {
                var v = values.Split(' ');
                var name = v[0];

                if (v.Length == 1)
                    return Game1.getCharacterFromName(name) is NPC npcp && npcp.currentLocation == location;

                var x = int.Parse(v[1]);

                if (v.Length == 2)
                    return Game1.getCharacterFromName(name) is NPC npcx && npcx.currentLocation == location && npcx.Tile.X == x;

                var y = int.Parse(v[2]);
                return Game1.getCharacterFromName(name) is NPC npc && npc.currentLocation == location && (x == -1 || npc.Tile.X == x) && (y == -1 || npc.Tile.Y == y);
            });

            PyUtils.addEventPrecondition("items", (key, values, location) =>
            {
                var v = values.Split(',');
                List<Item> items = new List<Item>(Game1.player.Items);
                foreach (string pair in v)
                {
                    var p = pair.Split(':');
                    var name = p[0];
                    var stack = p.Length == 1 ? 1 : int.Parse(p[1]);
                    int count = 0;

                    foreach (Item item in items)
                    {
                        if (item.Name == name)
                            count += item.Stack;

                        if (count >= stack)
                            return true;
                    }
                }

                return false;
            });

            PyUtils.addEventPrecondition("counter", (key, values, location) =>
            {
                var v = values.Split(' ');
                var c = LuaUtils.counters(v[0]);

                if (v.Length == 2)
                    return c == int.Parse(v[1]);
                else
                    return PyUtils.calcBoolean("c " + values, new KeyValuePair<string, object>("c", c));
            });

            PyUtils.addEventPrecondition("LC", (key, values, location) =>
            {
                return PyUtils.checkEventConditions(values.Replace("%div", "/"), location, location);
            });


        }

        private void PYTKGameLoop_DayStarted(object sender, DayStartedEventArgs e)
        {
            if (Game1.currentLocation is GameLocation g && g.map is Map m && m.Properties.ContainsKey("EntryAction"))
                TileAction.invokeCustomTileActions("EntryAction", g, Vector2.Zero, "Map");
        }


        private void startResponder()
        {
            responders.ForEach(r => r.start());
        }

        private void initializeResponders()
        {

            responders = new List<IPyResponder>();

            responders.Add(new PyReceiver<PyTKSaveData>("PyTK.ModSavdDataReceiver", (sd) =>
            {
                pytksaveData.Counters = sd.Counters;
            }, 60, SerializationType.JSON));

            responders.Add(new PyReceiver<ValueChangeRequest<string, int>>("PyTK.ModSavdDataCounterChangeReceiver", (cr) =>
            {
                if (!pytksaveData.Counters.ContainsKey(cr.Key))
                    pytksaveData.Counters.Add(cr.Key, cr.Fallback);
                else
                    pytksaveData.Counters[cr.Key] += cr.Value;
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
            }, 16, SerializationType.PLAIN, SerializationType.JSON));
        }

        private void Player_WarpedPyTK(object sender, WarpedEventArgs e)
        {
            if (e.NewLocation.Map.Properties.ContainsKey("@WaterColor") && TMXColor.FromString(e.NewLocation.Map.Properties["@WaterColor"]) is TMXColor color)
                e.NewLocation.waterColor.Value = new Color(color.R, color.G, color.B, color.A);

            if (!e.IsLocalPlayer)
                return;

            e.NewLocation?.Map.enableMoreMapLayers();

            if (e.NewLocation is GameLocation loc && loc.waterTiles == null && loc.Map.Properties.TryGetValue("@LoadWater", out xTile.ObjectModel.PropertyValue value) && (value == true || value.ToString() == "T"))
            {
                loc.waterTiles = new WaterTiles(new bool[loc.map.Layers[0].LayerWidth, loc.map.Layers[0].LayerHeight]);
                bool flag = false;
                for (int xTile = 0; xTile < loc.map.Layers[0].LayerWidth; ++xTile)
                {
                    for (int yTile = 0; yTile < loc.map.Layers[0].LayerHeight; ++yTile)
                    {
                        if (loc.doesTileHaveProperty(xTile, yTile, "Water", "Back") != null)
                        {
                            flag = true;
                            loc.waterTiles[xTile, yTile] = true;
                        }
                    }
                }
                if (!flag)
                    loc.waterTiles = new WaterTiles((bool[,])null);
            }

            if (e.NewLocation is GameLocation g && g.map is Map m)
            {
                int forceX = Game1.player.TilePoint.X;
                int forceY = Game1.player.TilePoint.Y;
                int forceF = Game1.player.FacingDirection;
                if (e.OldLocation is GameLocation og && m.Properties.ContainsKey("ForceEntry_" + og.Name))
                {
                    string[] pos = m.Properties["ForceEntry_" + og.Name].ToString().Split(' ');
                    if (pos.Length > 0 && pos[1] != "X")
                        int.TryParse(pos[0], out forceX);

                    if (pos.Length > 1 && pos[1] != "Y")
                        int.TryParse(pos[1], out forceY);

                    if (pos.Length > 2 && pos[2] != "F")
                        int.TryParse(pos[2], out forceF);

                    Game1.player.Position = new Vector2(forceX, forceY);
                    Game1.player.FacingDirection = forceF;
                }

                if (m.Properties.ContainsKey("EntryAction"))
                    TileAction.invokeCustomTileActions("EntryAction", g, Vector2.Zero, "Map");

                PyUtils.checkDrawConditions(m);
            }
        }

        private void OnAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            // load buildable interior
            if (e.DataType == typeof(Map) && this.BuildableInteriors.TryGetValue(e.NameWithoutLocale, out Map map))
                e.LoadFrom(() => map, AssetLoadPriority.Medium);

            // apply editors
            foreach (TMXAssetEditor editor in this.AssetEditors)
            {
                if (editor.CanEdit(e.NameWithoutLocale))
                    e.Edit(asset => editor.Edit(asset), onBehalfOf: editor.ContentPackId);
            }
        }

        private void waterEverythingForRainProperty()
        {
            foreach (var l in Game1.locations.Where(l => l.IsFarm))
                if(l.Map.Properties.TryGetValue("Raining", out PropertyValue rain))
                    if(rain.ToString() == "Never" && Game1.isRaining)
                        l.terrainFeatures.FieldDict.Values.Where(t => t.Value is HoeDirt h && h.state.Value == 1 ).Select(t => t.Value as HoeDirt).ToList().ForEach(h => h.state.Value = 0);
                    else if(rain.ToString() == "Always" && !Game1.isRaining)
                            l.terrainFeatures.FieldDict.Values.Where(t => t.Value is HoeDirt).Select(t => t.Value as HoeDirt).ToList().ForEach(h => h.state.Value = 1);
        }

        public static void syncCounter(string id, int value)
        {
            if (Game1.IsMultiplayer)
                PyNet.sendRequestToAllFarmers<bool>("PyTK.ModSavdDataCounterChangeReceiver", new ValueChangeRequest<string, int>(id, value, pytksaveData.Counters[id]), null, SerializationType.JSON, -1);
        }

        private void updateRainDrops()
        {
            if (Constants.TargetPlatform == GamePlatform.Android)
                return;

            for (int index = 0; index < Game1.rainDrops.Length; ++index)
            {
                if (AccessTools.Method(typeof(Game1), "updateRaindropPosition") is MethodInfo rainMethod)
                    rainMethod.Invoke(null,null);

                if (Game1.rainDrops[index].frame == 0)
                {
                    Game1.rainDrops[index].accumulator += Game1.currentGameTime.ElapsedGameTime.Milliseconds;
                    if (Game1.rainDrops[index].accumulator >= 70)
                    {
                        Game1.rainDrops[index].position += new Vector2((float)(index * 8 / Game1.rainDrops.Length - 16), (float)(32 - index * 8 / Game1.rainDrops.Length));
                        Game1.rainDrops[index].accumulator = 0;
                        if (Game1.random.NextDouble() < 0.1)
                            ++Game1.rainDrops[index].frame;
                        if ((double)Game1.rainDrops[index].position.Y > (double)(Game1.viewport.Height + 64))
                            Game1.rainDrops[index].position.Y = -64f;
                    }
                }
                else
                {
                    Game1.rainDrops[index].accumulator += Game1.currentGameTime.ElapsedGameTime.Milliseconds;
                    if (Game1.rainDrops[index].accumulator > 70)
                    {
                        Game1.rainDrops[index].frame = (Game1.rainDrops[index].frame + 1) % 4;
                        Game1.rainDrops[index].accumulator = 0;
                        if (Game1.rainDrops[index].frame == 0)
                            Game1.rainDrops[index].position = new Vector2((float)Game1.random.Next(Game1.viewport.Width), (float)Game1.random.Next(Game1.viewport.Height));
                    }
                }
            }
        }

        private void beforeSave()
        {
            if (Game1.IsMasterGame)
            {
                saveData = new SaveData();
                saveData.Locations = new List<SaveLocation>();
                saveData.Buildables = new List<SaveBuildable>();
                saveData.Data = new List<PersistentData>();

                foreach (var l in addedLocations)
                    if (Game1.getLocationFromName(l.name) is GameLocation location && getLocationSaveData(location) is SaveLocation sav)
                        saveData.Locations.Add(sav);

                foreach (var b in buildablesBuild)
                {
                    BuildableEdit edit = buildables.Find(be => be.id == b.Id);

                    if (edit.indoorsFile != null && Game1.getLocationFromName(getLocationName(b.UniqueId)) is GameLocation location && getLocationSaveData(location) is SaveLocation sav)
                        b.Indoors = sav;

                    saveData.Buildables.Add(b);
                }

                foreach (GameLocation location in Game1.locations)
                    if (location.Map.Properties.TryGetValue("PersistentData", out PropertyValue dataString)
                    && dataString != null
                    && dataString.ToString().Split(':') is string[] data && data.Length == 3)
                        saveData.Data.Add(new PersistentData(data[0], data[1], data[2]));

                Helper.Data.WriteSaveData<SaveData>("Locations", saveData);
            }

            locationStorage.Clear();

            foreach (var l in addedLocations)
                if (Game1.getLocationFromName(l.name) is GameLocation location)
                {
                    Game1.locations.Remove(location);
                    locationStorage.Add(location);
                }

            foreach (var b in buildablesBuild)
                if (buildables.Find(be => be.id == b.Id) is BuildableEdit edit && edit.indoorsFile != null && Game1.getLocationFromName(getLocationName(b.UniqueId)) is GameLocation location)
                {
                    Game1.locations.Remove(location);
                    locationStorage.Add(location);
                }
        }

        private void afterSave()
        {
            foreach (var loc in locationStorage)
                Game1.locations.Add(loc);
        }

        private void restoreAllSavedBuildables()
        {
            buildablesBuild = new List<SaveBuildable>();
            buildablesExits = new Dictionary<string, Warp>();

            foreach (var location in addedLocations)
            {
                Monitor?.Log("Restore Location: " + location.name);

                if (Game1.getLocationFromName(location.name) == null)
                    addLocation(location).updateSeasonalTileSheets();
            }
            if (!Game1.IsMasterGame)
                return;

            var ja = Helper.ModRegistry.GetApi<IJsonAssetsAPI>("spacechase0.JsonAssets");
            saveData = Helper.Data.ReadSaveData<SaveData>("Locations");
            if (saveData != null)
            {
                foreach (SaveLocation loc in saveData.Locations)
                {
                    Monitor?.Log("Restore Location objects: " + loc.Name);

                    setLocationObjects(loc);
                    try
                    {
                        if (ja != null && Game1.getLocationFromName(loc.Name) is GameLocation location)
                            ja.FixIdsInLocation(location);
                    }
                    catch
                    {

                    }
                }

                foreach (var b in saveData.Buildables)
                {
                    loadSavedBuildable(b);
                    try
                    {
                        if (ja != null && b.Indoors is SaveLocation sl && Game1.getLocationFromName(sl.Name) is GameLocation indoorsLocation)
                            ja.FixIdsInLocation(indoorsLocation);
                    }
                    catch
                    {

                    }
                }

                foreach (GameLocation location in Game1.locations)
                    loadPersistentDataToLocation(location);
            }
        }

        private void removeAllSavedBuildables()
        {
            List<SaveBuildable> removeList = new List<SaveBuildable>(buildablesBuild);
            foreach (var toRemove in removeList)
                removeSavedBuildable(toRemove,false, false);
        }
       
        internal void removeSavedBuildable(SaveBuildable toRemove, bool pay, bool distribute)
        {
            try
            {
                buildablesBuild.Remove(toRemove);
               
                if (pay && Game1.IsMasterGame && buildables.Find(b => b.id == toRemove.Id) is BuildableEdit be)
                {
                    GameLocation loc = Game1.getLocationFromName(toRemove.Location);
                    Map map = helper.GameContent.Load<Map>(loc.mapPath.Value);
                    Map bMap  = TMXContent.Load(be.file, Helper, be._pack);
                    if (bMap == null)
                        return;
                    loc.map = map.mergeInto(loc.Map, new Vector2(toRemove.Position[0], toRemove.Position[1]),new Microsoft.Xna.Framework.Rectangle(toRemove.Position[0], toRemove.Position[1],bMap.DisplayWidth / Game1.tileSize, bMap.DisplayHeight / Game1.tileSize), true);

                    List<Layer> layersToRemove = new List<Layer>();

                    foreach (var l in loc.map.Layers.Where(l => l.Id.Contains(toRemove.UniqueId)))
                        layersToRemove.Add(l);

                    foreach (Layer layer in layersToRemove)
                        loc.map.RemoveLayer(layer);

                    loc.map.LoadTileSheets(Game1.mapDisplayDevice);
                        loc.updateSeasonalTileSheets();
                        loc.map.enableMoreMapLayers();

                    Game1.player.Money += be.price;
                    List<Item> items = new List<Item>();

                    foreach (var buildItem in be.buildItems)
                    {
                        if (TMXActions.getItem(buildItem.type, buildItem.index, buildItem.name) is Item item)
                        {
                            item.Stack = buildItem.stack;
                            items.Add(item);
                        }
                    }

                    if (items.Count > 0)
                        Game1.player.addItemsByMenuIfNecessary(items);

                }
            }
            catch(Exception e)
            {
                Monitor?.Log(e.Message + ":" + e.StackTrace);
            }

            if (distribute && Game1.IsMultiplayer)
            {
                Monitor?.Log("Send Removal request");
                PyNet.sendRequestToAllFarmers<bool>(buildableRemoverName, toRemove, null, SerializationType.JSON, -1);
            }
        }
        private void loadSavedBuildable(SaveBuildable b)
        {
            Monitor?.Log("Restore Buildable: " + b.Id);

            if (Game1.getLocationFromName(b.Location) is GameLocation location)
            {
                BuildableEdit edit;

                if (buildables.Find(bd => bd.id == b.Id) is BuildableEdit build)
                    edit = build;
                else
                    return;

                buildBuildableEdit(false, edit, location, new Point(b.Position[0], b.Position[1]), b.Colors, b.UniqueId, b.PlayerName, b.PlayerId, false);
                if (b.Indoors != null)
                    setLocationObjects(b.Indoors);

            }
        }

        internal string getLocationName(string uniqueId)
        {
            return "BuildableIndoors" + "-" + uniqueId;
        }

        private GameLocation buildBuildableIndoors(BuildableEdit edit, string uniqueId, string playerName, long playerId, GameLocation location, Dictionary<string,string> colors)
        {
            if (Game1.getLocationFromName(getLocationName(uniqueId)) is GameLocation preLocation)
                return preLocation;

            if (edit.indoorsFile != null && edit._pack != null)
            {
                string buildFile = edit.indoorsFile;

                helper.GameContent.InvalidateCache(edit._pack.ModContent.GetInternalAssetName(buildFile));

                Map map = TMXContent.Load(buildFile, Helper, edit._pack);
                if (map == null)
                    return null;

                map = loadVariablesToMap(map, edit, new Point(edit.position[0],edit.position[1]), colors, uniqueId, playerName, playerId, location);
                var e = edit.Clone();
                e.name = getLocationName(uniqueId);
                e._map = map;

                if (!map.Properties.ContainsKey("Warp"))
                    map.Properties["Warp"] = "0 0 Farm 0 0";

                this.BuildableInteriors.TryAdd(Helper.GameContent.ParseAssetName($"Maps/{e.name}"), map);

                if (!map.Properties.ContainsKey("Group"))
                    map.Properties.Add("Group", "Buildables");

                if (!map.Properties.ContainsKey("Name"))
                    map.Properties.Add("Name", edit.name);

                return addLocation(e);
            }
            else
                return null;
        }

        internal void buildBuildableEdit(bool pay, BuildableEdit edit, GameLocation location, Point position, Dictionary<string, string> colors,  string uniqueId = null, string playerName = null, long playerId = -1, bool distribute = true)
        {
            if (uniqueId == null)
                    uniqueId = ((ulong)Helper.Multiplayer.GetNewID()).ToString();

            if(playerName == null || playerId == -1)
            {
                playerId = Game1.player.UniqueMultiplayerID;
                playerName = Game1.player.Name;
            }

            edit.position = new int[] { position.X, position.Y };

            GameLocation indoors = buildBuildableIndoors(edit, uniqueId, playerName,playerId, location, colors);

            if (indoors != null && !buildablesExits.ContainsKey(indoors.Name))
                buildablesExits.Add(indoors.Name, new Warp(0, 0, location.Name, edit.exitTile[0] + position.X, edit.exitTile[1] + position.Y, false));

          
            string buildFile = edit.file;

            Map map = edit._map;
            if (edit._pack != null)
            {
                map = TMXContent.Load(buildFile, Helper, edit._pack);
                if (map == null)
                    return;
            }

            var size = new Microsoft.Xna.Framework.Rectangle(0, 0, 1, 1);

            map = loadVariablesToMap(map, edit, position, colors, uniqueId, playerName, playerId, location);

            if (config.clearBuildingSpace && pay)
            {
                foreach(xTile.Layers.Layer layer in map.Layers.Where(l => l.Id == "Back" || l.Id == "Buildings"))
                {
                    size = new Microsoft.Xna.Framework.Rectangle(0, 0, layer.DisplayWidth / Game1.tileSize, layer.DisplayHeight / Game1.tileSize);

                    for (int x = 0; x < size.Width; x++)
                        for (int y = 0; y < size.Height; y++)
                        {
                            try
                            {
                                Vector2 key = new Vector2(x + position.X, y + position.Y);

                                if (layer.Id == "Back" && location.terrainFeatures.ContainsKey(key))
                                    location.terrainFeatures.Remove(key);


                                List<LargeTerrainFeature> ltfToRemove = new List<LargeTerrainFeature>();

                                foreach (var ltf in location.largeTerrainFeatures)
                                    if (LuaUtils.getDistance(ltf.Tile, key) < 4)
                                        ltfToRemove.Add(ltf);

                                foreach (var ltf in ltfToRemove)
                                    location.largeTerrainFeatures.Remove(ltf);

                                if (location is Farm farm)
                                {
                                    List<ResourceClump> rcToRemove = new List<ResourceClump>();

                                    foreach (var rc in farm.resourceClumps)
                                        if (rc.occupiesTile(x, y))
                                            rcToRemove.Add(rc);

                                    foreach (var rc in rcToRemove)
                                        farm.resourceClumps.Remove(rc);
                                }
                            }
                            catch
                            {

                            }
                        }
                }
            }

            edit._mapName = location.mapPath.Value;
            edit._location = location.Name;
            var e = edit.Clone();
            e._map = map;

            SaveBuildable sav = (new SaveBuildable(edit.id, location.Name, position, uniqueId, playerName,playerId, colors));

            if (edit.tags.Contains("IsUnique"))
                foreach (SaveBuildable sb in new List<SaveBuildable>(buildablesBuild.Where(bb => bb.Id == edit.id && bb.UniqueId != uniqueId)))
                {
                    BuildableEdit sbedit = buildables.Find(be => be.id == sb.Id);

                    if (sbedit.indoorsFile != null && Game1.getLocationFromName(getLocationName(sb.UniqueId)) is GameLocation sblocation && getLocationSaveData(sblocation) is SaveLocation savd)
                    {
                        var locSaveData = savd;
                        locSaveData.Name = getLocationName(uniqueId);
                        setLocationObjects(locSaveData);
                    }

                    removeSavedBuildable(sb, pay, distribute);
                }

            buildablesBuild.Add(sav);

            if (distribute && Game1.IsMultiplayer)
                PyNet.sendRequestToAllFarmers<bool>(buildableReceiverName, sav, null, SerializationType.JSON, -1);

            location.map = map.mergeInto(location.Map,position.GetVector2(),null,edit.removeEmpty);

            location.map.LoadTileSheets(Game1.mapDisplayDevice);

            foreach (Layer layer in map.Layers.Where(l => l.IsImageLayer()))
                if (location.map.Layers.FirstOrDefault(ll => ll.Id == layer.Id) is Layer il)
                {
                    var pos = il.GetOffset();
                    pos.X = pos.X + position.X * Game1.tileSize;
                    pos.Y = pos.Y + position.Y * Game1.tileSize;
                    il.SetOffset(pos);
                }
                    
            location.updateSeasonalTileSheets();
            location.map.enableMoreMapLayers();
            fixWaterTiles(location);

            if (pay)
            {
                Game1.player.Money -= edit.price;

                if (edit.buildItems.Count > 0)
                    foreach (TileShopItem tItem in edit.buildItems)
                    {
                        Item item = TMXActions.getItem(tItem.type, tItem.index, tItem.name);
                        if (item == null || !(item is StardewValley.Object))
                            continue;

                        if ((item as StardewValley.Object).bigCraftable.Value)
                            continue;

                        Game1.player.removeFirstOfThisItemFromInventory(item.ItemId, tItem.stock);
                    }
            }           
        }

        private void fixWaterTiles(GameLocation location)
        {
            if ((location.IsOutdoors || location is Sewer || location is Submarine) && !(location is Desert))
            {
                location.waterTiles = new WaterTiles(new bool[location.map.Layers[0].LayerWidth, location.map.Layers[0].LayerHeight]);
                bool flag = false;
                for (int xTile = 0; xTile < location.map.Layers[0].LayerWidth; ++xTile)
                {
                    for (int yTile = 0; yTile < location.map.Layers[0].LayerHeight; ++yTile)
                    {
                        if (location.doesTileHaveProperty(xTile, yTile, "Water", "Back") != null)
                        {
                            flag = true;
                            location.waterTiles[xTile, yTile] = true;
                        }
                    }
                }
                if (!flag)
                    location.waterTiles = new WaterTiles((bool[,])null);
            }
        }

        private Map loadVariablesToMap(Map map, BuildableEdit edit, Point position, Dictionary<string, string> colors, string uniqueId, string playerName, long playerId, GameLocation location)
        {
            var size = new Microsoft.Xna.Framework.Rectangle(0, 0, 1, 1);

            Func<KeyValuePair<string, PropertyValue>, bool> propCheck = (prop) =>
            {
                return prop.Value.ToString().Contains("PLAYERNAME") || prop.Value.ToString().Contains("PLAYERID") ||  prop.Value.ToString().Contains("BUILDLOCATION") || prop.Value.ToString().Contains("EXITXY") || prop.Value.ToString().Contains("XEXIT") || prop.Value.ToString().Contains("YEXIT") || prop.Value.ToString().Contains("POSXY") || prop.Value.ToString().Contains("XPOS") || prop.Value.ToString().Contains("YPOS") || prop.Value.ToString().Contains("UNIQUEID") || prop.Value.ToString().Contains("INDOORS");
            };

            Func<string, string, string> propChange = (key, value) =>
            {
                return value.Replace("PLAYERID", playerId.ToString()).Replace("PLAYERNAME", playerName).Replace("BUILDLOCATION", location.Name).Replace("EXITXY", (edit.exitTile.Length > 1 ? (edit.exitTile[0] + position.X) + " " + (edit.exitTile[1] + position.Y) : "0 0").ToString()).Replace("XEXIT", (edit.exitTile.Length > 0 ? edit.exitTile[0] + position.X : 0).ToString()).Replace("YEXIT", (edit.exitTile.Length > 0 ? edit.exitTile[1] + position.Y : 0).ToString()).Replace("INDOORS", getLocationName(uniqueId)).Replace("UNIQUEID", uniqueId).Replace("XPOS", position.X.ToString()).Replace("YPOS", position.Y.ToString()).Replace("POSXY", position.X + " " + position.Y);
            };
            List<string> keys = new List<string>();


            foreach (var property in map.Properties.Where(prop => propCheck(prop)))
                keys.Add(property.Key);

            foreach (string key in keys)
                map.Properties[key] = propChange(key, map.Properties[key]);

            foreach (xTile.Layers.Layer layer in map.Layers)
            {
                keys.Clear();
                foreach (var property in layer.Properties.Where(prop => propCheck(prop)))
                    keys.Add(property.Key);

                if (layer.Properties.ContainsKey("ColorId") && colors.ContainsKey(layer.Properties["ColorId"]))
                    layer.Properties["Color"] = colors[layer.Properties["ColorId"]];


                foreach (string key in keys)
                    layer.Properties[key] = propChange(key, layer.Properties[key]);

                if (layer.Id.Contains("UNIQUEID"))
                {
                    if (layer.IsImageLayer())
                    {
                        var offset = layer.GetOffset();
                        offset.X += position.X * 64;
                        offset.Y += position.Y * 64;
                        if (layer.Map.TileSheets.FirstOrDefault(t => t.Id == "zImageSheet_" + layer.Id) is TileSheet zs)
                            layer.SetTileSheetForImageLayer(zs);
                    }
                    layer.Id = layer.Id.Replace("UNIQUEID", uniqueId);
                }

                size = new Microsoft.Xna.Framework.Rectangle(0, 0, layer.DisplayWidth / Game1.tileSize, layer.DisplayHeight / Game1.tileSize);


                for (int x = 0; x < size.Width; x++)
                    for (int y = 0; y < size.Height; y++)
                    {
                        try
                        {
                            Location tileLocation = new Location(x, y);
                            Tile tile = layer.Tiles[tileLocation];

                            if (tile == null)
                                continue;

                            keys.Clear();

                            foreach (var property in tile.Properties.Where(prop => propCheck(prop)))
                                keys.Add(property.Key);

                            foreach (string key in keys)
                                tile.Properties[key] = propChange(key, tile.Properties[key]);
                        }
                        catch (Exception ex)
                        {
                            Monitor?.Log(ex.Message + ":" + ex.StackTrace);
                            continue;
                        }
                    }
            }

            return map;
        }

        internal bool setLocationObjects(SaveLocation loc)
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.ConformanceLevel = ConformanceLevel.Auto;

            var inGame = Game1.getLocationFromName(loc.Name);
            if (inGame == null)
                return false;

            StringReader objReader = new StringReader(loc.Objects);
            GameLocation saved;

            using (var reader = XmlReader.Create(objReader, settings))
            {
                try
                {
                    saved = (GameLocation)SerializationFix.SafeDeSerialize(reader, inGame);
                }
                catch (Exception e)
                {
                    Monitor?.Log("Failed to deserialize: " + loc.Name, LogLevel.Warn);
                    Monitor?.Log(e.Message, LogLevel.Info);
                    Monitor?.Log(e.StackTrace);
                    return false;
                }
            }

            if (saved != null)
            {
                inGame.Objects.Clear();
                if (saved.Objects.Count() > 0)
                    foreach (var obj in saved.Objects.Keys)
                        inGame.Objects.Add(obj, saved.Objects[obj]);

                inGame.terrainFeatures.Clear();
                if (saved.terrainFeatures.Count() > 0)
                    foreach (var obj in saved.terrainFeatures.Keys)
                        inGame.terrainFeatures.Add(obj, saved.terrainFeatures[obj]);

                inGame.debris.Clear();
                if (saved.debris.Count() > 0)
                    foreach (var obj in saved.debris)
                        inGame.debris.Add(obj);

                inGame.largeTerrainFeatures.Clear();
                if (saved.largeTerrainFeatures.Count > 0)
                    foreach (var obj in saved.largeTerrainFeatures)
                        inGame.largeTerrainFeatures.Add(obj);

                if (inGame is DecoratableLocation dl)
                {
                    dl.furniture.Clear();
                    for (int i = 0; i < dl.wallPaper.Count(); i++)
                        dl.wallPaper[i] = 0;

                    for (int i = 0; i < dl.floor.Count(); i++)
                        dl.floor[i] = 0;

                    if (saved is DecoratableLocation sdl)
                    {
                        foreach (Furniture f in sdl.furniture)
                        {
                            dl.furniture.Add(f);
                            dl.moveFurniture((int)f.TileLocation.X, (int)f.TileLocation.Y, (int)f.TileLocation.X, (int)f.TileLocation.Y);
                        }

                        for (int i = 0; i < sdl.wallPaper.Count(); i++)
                            dl.wallPaper[i] = dl.wallPaper[i];

                        for (int i = 0; i < dl.floor.Count(); i++)
                            dl.floor[i] = 0;

                        dl.setWallpapers();
                        dl.setFloors();
                    }
                }

                if (inGame is IAnimalLocation al && saved is IAnimalLocation sal)
                {
                    al.Animals.Clear();
                    foreach (var a in sal.Animals.FieldDict.Keys.Where(k => sal.Animals[k] is FarmAnimal))
                    {
                        al.Animals.Add(a, sal.Animals[a]);
                    }
                }

                if (true)
                {
                    inGame.buildings.Clear();
                    foreach (Building b in inGame.buildings)
                    {
                        inGame.buildings.Add(b);
                        b.load();
                    }
                }
            }

            //PyTK.CustomElementHandler.SaveHandler.RebuildAll(inGame, Game1.locations);
            TMXLAPI.RaiseOnLocationRestoringEvent(inGame);
            inGame.DayUpdate(Game1.dayOfMonth);
            if(inGame is FarmHouse)
                inGame.resetForPlayerEntry();
            return true;
        }


        internal SaveLocation getLocationSaveData(GameLocation location)
        {
           // PyTK.CustomElementHandler.SaveHandler.ReplaceAll(location, Game1.locations);
            string objects = "";
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.ConformanceLevel = ConformanceLevel.Auto;
            settings.CloseOutput = true;
            StringWriter objWriter = new StringWriter();

            foreach (Monster monster in location.characters.Where(c => c is Monster && c.GetType().Name.Contains("FTM")).ToList())
                location.characters.Remove(monster);

            foreach (LargeTerrainFeature feature in location.largeTerrainFeatures.Where(t => t.GetType().Name.Contains("LargeResourceClump")).ToList())
                location.largeTerrainFeatures.Remove(feature);

            foreach (Vector2 key in location.Objects.Keys.Where(k => location.Objects[k].GetType().Name.Contains("FTM")).ToList())
                location.Objects.Remove(key);

            using (var writer = XmlWriter.Create(objWriter, settings))
            {
                try
                {
                    SerializationFix.SafeSerialize(writer, location);
                }
                catch (Exception e)
                {
                    Monitor?.Log("Failed to serialize: " + location.Name, LogLevel.Warn);
                    Monitor?.Log(e.Message, LogLevel.Info);
                    Monitor?.Log(e.StackTrace);
                    return null;
                }
            }
            objects = objWriter.ToString();

            return new SaveLocation(location.Name, objects);
        }

        private string getAssetNameWithoutFolder(string asset)
        {
            return Path.GetFileNameWithoutExtension(asset).Split('/').Last().Split('\\').Last();
        }

        private void Player_Warped(object sender, WarpedEventArgs e)
        {
            if (!e.IsLocalPlayer || !(e.NewLocation is GameLocation))
                return;

            if (resetRain)
            {
                Game1.isRaining = resetRainValue;
                e.NewLocation.resetForPlayerEntry();
                resetRain = false;
            }

            if (Game1.isRaining && e.NewLocation.Map.Properties.TryGetValue("Raining", out PropertyValue rain) && rain.ToString() == "Never")
            {
                resetRainValue = Game1.isRaining;
                Game1.isRaining = false;
                e.NewLocation.resetForPlayerEntry();
                resetRain = true;
            }

            if (!Game1.isRaining && e.NewLocation.Map.Properties.TryGetValue("Raining", out PropertyValue raina) && raina.ToString() == "Always")
            {
                resetRainValue = Game1.isRaining;
                Game1.isRaining = true;
                e.NewLocation.resetForPlayerEntry();
                resetRain = true;
            }

            foreach (TMXAssetEditor editor in conditionals)
                if (e.NewLocation is GameLocation gl && gl.mapPath.Value is string mp)
                {

                    if (!getAssetNameWithoutFolder(mp).Equals(getAssetNameWithoutFolder(editor.assetName)))
                        continue;

                    if (PyUtils.checkEventConditions(editor.conditions) is bool c)
                    {
                        bool inlocation = (editor.inLocation == null || editor.inLocation == gl.Name);
                        bool r = c && inlocation;

                        if (r != editor.lastCheck)
                        {
                            editor.lastCheck = r;

                            Helper.GameContent.InvalidateCache(e.NewLocation.mapPath.Value);
                            helper.GameContent.InvalidateCache(Game1.currentLocation.mapPath.Value);
                            try
                            {
                                Game1.currentLocation.reloadMap();
                                Game1.currentLocation.GetType().GetField("_appliedMapOverrides", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(Game1.currentLocation, new HashSet<string>());
                                Game1.currentLocation.GetType().GetField("ccRefurbished", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(Game1.currentLocation, false);
                                Game1.currentLocation.GetType().GetField("isShowingDestroyedJoja", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(Game1.currentLocation, false);
                                Game1.currentLocation.GetType().GetField("isShowingUpgradedPamHouse", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(Game1.currentLocation, false);
                                Game1.currentLocation.GetType().GetField("isShowingDestroyedJoja", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(Game1.currentLocation, false);

                                Helper.Reflection.GetMethod(Game1.currentLocation, "resetLocalState")?.Invoke();
                                foreach (var buildable in new List<SaveBuildable>(buildablesBuild.Where(b => b.Location == Game1.currentLocation.Name)))
                                {
                                    buildablesBuild.Remove(buildable);
                                    BuildableEdit edit = buildables.Find(be => be.id == buildable.Id);

                                    if (edit.indoorsFile != null && Game1.getLocationFromName(getLocationName(buildable.UniqueId)) is GameLocation location && getLocationSaveData(location) is SaveLocation sav)
                                        buildable.Indoors = sav;

                                    loadSavedBuildable(buildable);
                                }
                            }
                            catch (Exception ex)
                            {
                                Monitor?.Log(ex.Message + ":" + ex.StackTrace);
                            }
                            e.NewLocation.updateSeasonalTileSheets();

                            PyUtils.checkDrawConditions(e.NewLocation.map);

                            e.NewLocation.map.enableMoreMapLayers();

                        }
                    }


                }

            foreach (string map in festivals)
                if (e.NewLocation is GameLocation gl && gl.mapPath.Value is string mp && mp.Contains(map))
                    Helper.GameContent.InvalidateCache(e.NewLocation.mapPath.Value);

            if(Game1.IsMasterGame)
                loadPersistentDataToLocation(e.NewLocation);

            if (e.NewLocation is GameLocation && e.NewLocation.Map is Map m)
                m.enableMoreMapLayers();
        }

        private static void loadPersistentDataToLocation(GameLocation location)
        {
            try
            {
                if(location is GameLocation)
                foreach (var d in saveData.Data.Where(p => p.Key == location.Name))
                {
                    if (!location.Map.Properties.ContainsKey("PersistentData"))
                        location.Map.Properties.Add("PersistentData", d.Type + ":" + d.Key + ":" + d.Value);
                    else if (!location.Map.Properties["PersistentData"].ToString().Split(';').Contains(d.Type + ":" + d.Key + ":" + d.Value))
                        location.Map.Properties["PersistentData"] = location.Map.Properties["PersistentData"].ToString() + ";" + d.Type + ":" + d.Key + ":" + d.Value;

                    if (
                        d.Type == "lock" &&
                        (!location.Map.Properties.TryGetValue("Unlocked", out PropertyValue unlocked)
                        || !unlocked.ToString().Split(';').ToList().Contains(d.Value))
                        )
                    {
                        if (location.Map.Properties.TryGetValue("Unlocked", out PropertyValue unlocked2))
                            location.Map.Properties["Unlocked"] = location.Map.Properties["Unlocked"].ToString() + ";" + d.Value;
                        else
                            location.Map.Properties.Add("Unlocked", d.Value);

                        string[] lockData = d.Value.Split('_');
                        int x = int.Parse(lockData[1]);
                        int y = int.Parse(lockData[2]);
                        Tile tile = location.Map.GetLayer(lockData[0]).Tiles[x, y];

                        if (tile == null)
                            continue;

                        if (tile.Properties.ContainsKey("Recall"))
                        {
                            if (lockData.Length >= 4 && lockData[3] == "recall")
                                tile.Properties["Action"] = tile.Properties["Recall"];
                            else
                                TileAction.invokeCustomTileActions("Recall", location, new Vector2(x, y), lockData[0]);
                        }
                        else
                        {
                            if (lockData.Length >= 4 && lockData[3] == "recall")
                                tile.Properties["Action"] = tile.Properties["Success"];
                            else
                                TileAction.invokeCustomTileActions("Success", location, new Vector2(x, y), lockData[0]);
                        }
                    }
                }

            }
            catch
            {

            }
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            if (config.converter)
                exportAllMaps();

            setTileActions();
            loadContentPacks();

            PyLua.registerType(typeof(Map), false, true);
            PyLua.registerType(typeof(TMXActions), false, false);
            PyLua.addGlobal("TMX", new TMXActions());
            

            helper.ConsoleCommands.Add("export_map", "Exports current map as tmx file", (s, p) =>
             {
                 if (Context.IsWorldReady && Game1.currentLocation is GameLocation location && location.Map is Map map)
                 {
                     string exportFolderPath = Path.Combine(helper.DirectoryPath, "Exports");
                     if (Directory.Exists(exportFolderPath))
                         Directory.CreateDirectory(exportFolderPath);

                     TMXContent.Save(location.Map, Path.Combine(exportFolderPath, Path.GetFileNameWithoutExtension(location.mapPath.Value) + ".tmx"), true, Monitor);
                 }
             });

            fixCompatibilities();
            harmonyFix();

            helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;

            var SaveAnywhere = helper.ModRegistry.GetApi<Omegasis.SaveAnywhere.Framework.ISaveAnywhereAPI>("Omegasis.SaveAnywhere");
            if (SaveAnywhere != null)
            {
                SaveAnywhere.BeforeSave += (_s, _e) => beforeSave();
                SaveAnywhere.AfterSave += (_s, _e) => afterSave();
                SaveAnywhere.AfterLoad += (_s, _e) => SavePatch();
            }
        }

        private void GameLoop_UpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            loadContentPacksLate();
            helper.Events.GameLoop.UpdateTicked -= GameLoop_UpdateTicked;
        }

        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (e.IsOneSecond && Context.IsWorldReady && Game1.IsMasterGame && Game1.IsMultiplayer && Game1.otherFarmers.Values.Where(f => f.isActive() && f != Game1.player && !syncedFarmers.Contains(f)) is IEnumerable<Farmer> ef && ef.Count() is int i && i > 0)
                syncMaps(ef);
        }

        private void syncMaps(IEnumerable<Farmer> farmers)
        {
            foreach (Farmer farmer in farmers)
            {
                foreach (KeyValuePair<string, Map> map in mapsToSync)
                    PyNet.syncMap(map.Value, map.Key, farmer);

                syncedFarmers.Remove(farmer);
                syncedFarmers.Add(farmer);
            }
        }

        private void fixCompatibilities()
        {
            helper.Events.GameLoop.SaveLoaded += (s, e) => Compatibility.CustomFarmTypes.fixGreenhouseWarp();
        }

        private void harmonyFix()
        {
            Harmony instance = new Harmony("Platonymous.TMXLoader");
            instance.Patch(typeof(SaveGame).GetMethod(nameof(SaveGame.loadDataToLocations)), postfix: new HarmonyMethod(this.GetType().GetMethod(nameof(SavePatch), BindingFlags.Public | BindingFlags.Static)));
            instance.Patch(typeof(GameLocation).GetMethod(nameof(GameLocation.setMapTile)), prefix: new HarmonyMethod(this.GetType().GetMethod(nameof(trySetMapTile), BindingFlags.Public | BindingFlags.Static)));
            
            instance.Patch(
                original: AccessTools.Method(typeof(Crop),nameof(Crop.harvest)),
                prefix: new HarmonyMethod(AccessTools.Method(this.GetType(), nameof(harvest))),
                postfix: new HarmonyMethod(AccessTools.Method(this.GetType(), nameof(harvestPost)))
                );


            instance.Patch(
                original: AccessTools.Method(typeof(HoeDirt), nameof(HoeDirt.performToolAction)),
                prefix: new HarmonyMethod(AccessTools.Method(this.GetType(), nameof(performToolAction)))
                );
            instance.Patch(
               original: AccessTools.Method(typeof(HoeDirt), nameof(HoeDirt.draw), new Type[] { typeof(SpriteBatch) }),
               prefix: new HarmonyMethod(AccessTools.Method(this.GetType(), nameof(drawHoeDirt))),
               postfix: new HarmonyMethod(AccessTools.Method(this.GetType(), nameof(drawHoeDirtPost)))
               );

            if (!helper.ModRegistry.IsLoaded("Entoarox.FurnitureAnywhere"))
                return;
            if (Type.GetType("Entoarox.FurnitureAnywhere.ModEntry, FurnitureAnywhere") is Type Me && Me.GetMethod("SleepFurniture", BindingFlags.NonPublic | BindingFlags.Instance) is MethodInfo SF)
                instance.Patch(SF, new HarmonyMethod(this.GetType().GetMethod("GetFA", BindingFlags.Public | BindingFlags.Static)));
        }

        private static bool skipTrySetMapTile = false;

        public static void drawHoeDirt(HoeDirt __instance, ref int __state)
        {
            __state = -1;

            if (__instance.crop is Crop crop && !crop.forageCrop.Value && (crop.whichForageCrop.Value == "-91" || crop.whichForageCrop.Value == "-101"))
            {
                __state = __instance.state.Value;
                __instance.state.Value = 2;
            }
        }

        public static void drawHoeDirtPost(HoeDirt __instance, ref int __state)
        {
            if (__state != -1)
                __instance.state.Value = __state;
        }

        public static bool performToolAction(HoeDirt __instance, ref bool __result)
        {
            if (__instance.crop is Crop crop && !crop.forageCrop.Value && 
                (crop.whichForageCrop.Value == "-90" || crop.whichForageCrop.Value == "-91" || crop.whichForageCrop.Value == "-100" || crop.whichForageCrop.Value == "-101"))
            {
                __result = false;
                return false;
            }

            return true;
        }

        public static bool harvest(Crop __instance, int xTile, int yTile, JunimoHarvester junimoHarvester, ref bool __result, ref bool __state)
        {
            __state = false;
            if (!__instance.forageCrop.Value && (__instance.whichForageCrop.Value == "-91" || __instance.whichForageCrop.Value == "-90"))
            {
                __result = false;

                Vector2 pos = new Vector2(xTile, yTile);

                if (junimoHarvester == null
                    && Game1.currentLocation.terrainFeatures.TryGetValue(pos, out TerrainFeature f)
                    && f is HoeDirt h
                    && h.crop == __instance)
                    foreach (Layer layer in Game1.currentLocation.Map.Layers.Where(l => l.Id.ToLower().StartsWith(Game1.currentSeason.ToLower() + "_crops")))
                        if (layer.Tiles[xTile, yTile] is Tile tile && layer.Properties.ContainsKey("HarvestAction"))
                        {
                            tile.Properties["HarvestAction"] = layer.Properties["HarvestAction"];
                            TileAction.invokeCustomTileActions("HarvestAction", Game1.currentLocation, pos, layer.Id);
                            __state = true;
                            break;
                        }

                return false;
            }

            return true;
        }

        public static void harvestPost(Crop __instance, int xTile, int yTile, ref bool __result, ref bool __state)
        {
            if (__state || !__result)
                return;


            if (!__instance.forageCrop.Value &&
                (__instance.whichForageCrop.Value == "-90" || __instance.whichForageCrop.Value == "-91" || __instance.whichForageCrop.Value == "-100" || __instance.whichForageCrop.Value == "-101"))
            {
                var pos = new Vector2(xTile, yTile);
                if (Game1.currentLocation.terrainFeatures.TryGetValue(pos, out TerrainFeature f)
                    && f is HoeDirt h
                    && (h.crop == null || (h.crop == __instance && !__instance.RegrowsAfterHarvest())))
                {
                    PyUtils.setDelayedAction(2,() => Game1.currentLocation.terrainFeatures.Remove(pos));
                }
            }
        }

        private static string SetMapTileLogKey = null;
        public static bool trySetMapTile(GameLocation __instance, int tileX, int tileY, int index, string layer, string action, int whichTileSheet = 0)
        {
            if (skipTrySetMapTile)
            {
                skipTrySetMapTile = false;
                return true;
            }

            try
            {

                skipTrySetMapTile = true;

                // log message for first tile in a location
                {
                    string logKey = $"{__instance.NameOrUniqueName}|{Game1.ticks}";
                    if (SetMapTileLogKey != logKey)
                    {
                        SetMapTileLogKey = logKey;
                        monitor.Log($"Setting map tiles for {__instance.NameOrUniqueName}...", LogLevel.Trace);
                    }
                }

                // log verbose message for each tile
                if (monitor.IsVerbose)
                    monitor?.Log($"Setting MapTile: X:{tileX} Y:{tileY} Layer:{layer} TileSheet: {__instance.Map.TileSheets[whichTileSheet].Id} ({whichTileSheet}) Location:{__instance.Name}", LogLevel.Trace);

                // set tile
                __instance.setMapTile(tileX, tileY, index, layer, action, whichTileSheet);
                return false;
            }
            catch
            {
                monitor?.Log($"Error setting MapTile: X:{tileX} Y:{tileY} Layer:{layer} TileSheet: {__instance.Map.TileSheets[whichTileSheet].Id} ({whichTileSheet}) Location:{__instance.Name}", LogLevel.Warn);
                monitor?.Log("----Tilesheets----", LogLevel.Trace);
                int i = 0;
                foreach (TileSheet ts in __instance.Map.TileSheets)
                    monitor?.Log(ts.Id + " ("+ i++ +")", LogLevel.Trace);

                try {
                    skipTrySetMapTile = true;
                    monitor?.Log("Trying to save it...", LogLevel.Info);
                    __instance.setMapTile(tileX, tileY, index, layer, action, whichTileSheet + 1);
                    monitor?.Log("...Done", LogLevel.Info);

                }
                catch
                {
                    monitor?.Log("...Failed", LogLevel.Error);
                }

                return false;
            }
        }

        public static void GetFA(Mod __instance)
        {
            faInstance = __instance;
        }

        public static void SavePatch()
        {
            try
            {
                foreach (var edit in addedLocations)
                {
                    _instance.Monitor?.Log("Add Location: " + edit.name);
                    if (!(Game1.getLocationFromName(edit.name) is GameLocation))
                        addLocation(edit);
                }

                _instance.restoreAllSavedBuildables();
            }
            catch(Exception e)
            {
                _instance.Monitor?.Log(e.Message, LogLevel.Trace);
                _instance.Monitor?.Log(e.StackTrace, LogLevel.Trace);
            }
        }

        private void setTileActions()
        {


            PyUtils.addTileAction("LoadMap", (key, values, location, position, layer) =>
            {
                string[] st = values.Split(' ');
                Game1.player.warpFarmer(new Warp(int.Parse(st[1]), int.Parse(st[2]), st[0], int.Parse(st[1]), int.Parse(st[2]), false));
                return true;
            } );

            PyUtils.addTileAction("DropIn", (key, values, location, position, layer) =>
            {
                string[] st = values.Split(' ');
                if (st.Length < 1)
                    return false;

                if (st.Length > 1 && Game1.getFarmer(long.Parse(st[1])) != Game1.player)
                    return false;

                if (Game1.player.ActiveObject is Item i)
                {
                    Game1.playSound("smallSelect");
                    Game1.player.removeItemFromInventory(i);
                    return TMXActions.addToItemList(st[0], i);
                }
                else
                    return false;
            });

            PyUtils.addTileAction("ExitBuildable", (key, values, location, position, layer) =>
             {
                 Monitor?.Log("WarpOut");
                 if (!buildablesExits.ContainsKey(location.Name))
                     return false;

                 Game1.player.warpFarmer(buildablesExits[location.Name]);
                 return true;
             });

            TileAction Lock = new TileAction("Lock", TMXActions.lockAction).register();
            TileAction Say = new TileAction("Say", TMXActions.sayAction).register();
            TileAction SwitchLayers = new TileAction("SwitchLayers", TMXActions.switchLayersAction).register();
            TileAction CopyLayers = new TileAction("CopyLayers", TMXActions.copyLayersAction).register();
            TileAction SpawnTreasure = new TileAction("SpawnTreasure", TMXActions.spawnTreasureAction).register();
            TileAction WarpHome = new TileAction("WarpHome", TMXActions.warpHomeAction).register();
            TileAction WarpInto = new TileAction("WarpInto", TMXActions.warpIntoAction).register();
            TileAction WarpFrom = new TileAction("WarpFrom", TMXActions.warpFromAction).register();
            TileAction Confirm = new TileAction("Confirm", TMXActions.confirmAction).register();
            TileAction OpenShop = new TileAction("OpenShop", TMXActions.shopAction).register();
        }

        internal static GameLocation addLocation(MapEdit edit)
        {
            GameLocation location;
            monitor?.Log("Adding:" + edit.name, LogLevel.Trace);
            if (edit.type == "Deco")
                location = new DecoratableLocation(Path.Combine("Maps", edit.name), edit.name);
            else if (edit.type == "Summit")
                location = new Summit(Path.Combine("Maps", edit.name), edit.name);
            else if (edit.type == "Farm")
                location = new Farm(Path.Combine("Maps", edit.name), edit.name);
            else if (edit.type == "Cellar")
                location = new Cellar(Path.Combine("Maps", edit.name), edit.name);
            else if (edit.type == "Shed")
                location = new Shed(Path.Combine("Maps", edit.name), edit.name);
            else if (edit.type.StartsWith("SDV:"))
            {
                location = (GameLocation)PyUtils.getTypeSDV(edit.type.Substring(4)).GetConstructor(new Type[] { typeof(string), typeof(string) }).Invoke(new object[] { Path.Combine("Maps", edit.name), edit.name });
                monitor?.Log("Type:" + (edit.type.Substring(4)), LogLevel.Trace);
            }
            else if (edit.type.StartsWith("Custom:"))
            {
                location = (GameLocation)Type.GetType(edit.type.Substring(7)).GetConstructor(new Type[] { typeof(string), typeof(string) }).Invoke(new object[] { Path.Combine("Maps", edit.name), edit.name });
                monitor?.Log("Type:" + edit.type.Substring(7), LogLevel.Trace);
            }
            else
                location = new GameLocation(Path.Combine("Maps", edit.name), edit.name);

            if (edit._map.Properties.ContainsKey("Outdoors") && edit._map.Properties["Outdoors"] == "F")
            {
                location.IsOutdoors = false;
                try
                {
                    location.loadLights();
                }
                catch (Exception ex)
                {
                    monitor?.Log(ex.Message + ":" + ex.StackTrace);

                }
                location.IsOutdoors = false;

            }

            if (edit._map.Properties.ContainsKey("IsGreenHouse"))
                location.IsGreenhouse = true;

            if (edit._map.Properties.ContainsKey("IsStructure"))
                location.isStructure.Value = true;

            if (edit._map.Properties.ContainsKey("IsFarm"))
                location.IsFarm = true;


            if (Game1.locations.Contains(location))
                Game1.locations.Remove(location);

            if (!Game1.locations.Contains(location))
                Game1.locations.Add(location);

            monitor?.Log("Successfully added:" + location.Name);

            return location;

        }

        public static void setupCrops(GameLocation location)
        {
            Dictionary<string, CropData> cropsDict = Game1.content.Load<Dictionary<string, CropData>>("Data\\Crops");

            foreach (Layer layer in location.Map.Layers)
                if (layer.Id.ToLower().StartsWith(Game1.currentSeason.ToLower() + "_crops") || layer.Id.ToLower().StartsWith("all_crops"))
                {
                    if (layer.Properties.ContainsKey("Conditions") && !PyUtils.checkEventConditions(layer.Properties["Conditions"].ToString()))
                        continue;

                    int startPhase = 0;

                    if (layer.Properties.TryGetValue("StartPhase", out PropertyValue sp))
                        int.TryParse(sp.ToString(), out startPhase);

                    bool canBeHarvested =
                            layer.Properties.TryGetValue("CanBeHarvested", out PropertyValue ch)
                            && (ch.ToString().ToLower() == "t" || ch.ToString().ToLower() == "true");

                    bool hideSoil =
                            layer.Properties.TryGetValue("HideSoil", out PropertyValue hs)
                            && (hs.ToString().ToLower() == "t" || hs.ToString().ToLower() == "true");

                    bool ignoreSeason =
                            layer.Properties.TryGetValue("IgnoreSeason", out PropertyValue ise)
                            && (ise.ToString().ToLower() == "t" || ise.ToString().ToLower() == "true");

                    bool autoWater =
                            layer.Properties.TryGetValue("AutoWater", out PropertyValue aw)
                            && (aw.ToString().ToLower() == "t" || aw.ToString().ToLower() == "true");


                    for (int x = 0; x < layer.DisplayWidth / layer.TileWidth; x++)
                        for (int y = 0; y < layer.DisplayHeight / layer.TileHeight; y++)
                        {
                            Tile tile = layer.Tiles[x, y];
                            if (tile == null)
                                continue;

                            string index = tile.TileIndex.ToString();

                                if (tile.Properties.TryGetValue("name", out PropertyValue name))
                                {
                                    if (name != null)
                                        if (Game1.objectData.Keys.FirstOrDefault(k => k == name || Game1.objectData[k].Name == name) is string key)
                                            index = key;
                                }
                                else if (tile.Properties.TryGetValue("Name", out PropertyValue name2))
                                {
                                    if (name != null)
                                        if (Game1.objectData.Keys.FirstOrDefault(k => k == name2 || Game1.objectData[k].Name == name2) is string key)
                                            index = key;
                                }
                                else
                                if (tile.Properties.TryGetValue("Id", out PropertyValue name3))
                                {
                                    if (name != null)
                                        if (Game1.objectData.Keys.FirstOrDefault(k => k == name3 || Game1.objectData[k].Name == name3) is string key)
                                            index = key;
                                }
                                else
                                if (tile.Properties.TryGetValue("id", out PropertyValue name4))
                                {
                                    if (name != null)
                                        if (Game1.objectData.Keys.FirstOrDefault(k => k == name4 || Game1.objectData[k].Name == name4) is string key)
                                            index = key;
                                }

                            Vector2 pos = new Vector2(x, y);
                            Crop crop = null;

                            if (index == "770")
                            {
                                var season = Game1.currentSeason.ToCharArray();
                                season[0] = season.ToString().ToUpper()[0];
                                string s = new string(season);
                                crop = new Crop(Crop.getRandomLowGradeCropForThisSeason(Enum.TryParse(s, out Season se) ? se : Season.Spring), x, y, location);
                            }
                            else if (cropsDict.ContainsKey(index))
                                crop = new Crop(index.ToString(), x, y, location);
                            else if (cropsDict.Any(kv => kv.Value.HarvestItemId == index || kv.Key == index))
                            {
                                index = cropsDict.FirstOrDefault(kv => kv.Value.HarvestItemId == index || kv.Key == index).Key;
                                crop = index != null ? new Crop(index.ToString(), x, y, location) : crop;
                            }
                            
                            

                                if (crop != null)
                            {
                                if (!crop.IsInSeason(location) && !ignoreSeason)
                                {
                                    if (location.terrainFeatures.ContainsKey(pos) && location.terrainFeatures[pos] is HoeDirt h)
                                        location.terrainFeatures.Remove(pos);

                                    continue;
                                }

                                if (!canBeHarvested)
                                    crop.whichForageCrop.Value = hideSoil ? "-91" : "-90";
                                else
                                    crop.whichForageCrop.Value = hideSoil ? "-101" : "-100";

                                if (startPhase >= crop.phaseDays.Count - 1)
                                    crop.growCompletely();
                                else
                                    crop.currentPhase.Value = startPhase;

                                HoeDirt hoedirt = new HoeDirt(Game1.isRaining ? 1 : 0, location);
                                
                                if (location.terrainFeatures.ContainsKey(pos))
                                {
                                    if (location.terrainFeatures[pos] is HoeDirt h && h.crop is Crop c && !c.dead.Value && (c.indexOfHarvest.Value == crop.indexOfHarvest.Value || (index == "770" && Game1.dayOfMonth != 1)))
                                    {
                                        if(c.dead.Value)
                                            location.terrainFeatures.Remove(pos);
                                        else if (c.currentPhase.Value < startPhase)
                                        {
                                            if (startPhase >= c.phaseDays.Count - 1)
                                                c.growCompletely();
                                            else
                                                c.currentPhase.Value = startPhase;

                                            if (!canBeHarvested)
                                                c.whichForageCrop.Value = hideSoil ? "-91" : "-90";
                                            else
                                                c.whichForageCrop.Value = hideSoil ? "-101" : "-100";
                                        }
                                    }
                                    else
                                        location.terrainFeatures.Remove(pos);
                                }

                                if (location.isTileLocationOpen(pos))
                                {
                                    location.terrainFeatures.Add(pos, hoedirt);
                                    hoedirt.crop = crop;
                                    hoedirt.tickUpdate(Game1.currentGameTime);
                                }

                                if (location.terrainFeatures.ContainsKey(pos) && location.terrainFeatures[pos] is HoeDirt hd && (Game1.isRaining || autoWater))
                                {
                                    hd.state.Value = 1;
                                    hd.tickUpdate(Game1.currentGameTime);
                                }
                            }
                            
                        }
                }
        }

        public override object GetApi()
        {
            return new TMXLAPI();
        }

        private IEnumerable<IContentPack> GetContentPacks()
        {
            foreach (IContentPack pack in Helper.ContentPacks.GetOwned())
                yield return pack;

            foreach (IContentPack pack in AddedContentPacks)
                yield return pack;
        }

        public void loadPack(IContentPack pack, string contentFile = "content")
        {
            TMXContentPack tmxPack = pack.ReadJsonFile<TMXContentPack>(contentFile + ".json");

            tmxPack.parent = pack;

            if (tmxPack.loadLate)
            {
                latePacks.Add(tmxPack);
                return;
            }

            loadPack(tmxPack);

            foreach (string alsoLoad in tmxPack.alsoLoad)
                loadPack(pack, alsoLoad);
        }

        internal void loadPack(TMXContentPack tmxPack) 
        {
            var pack = tmxPack.parent;
            string packModId = pack.Manifest.UniqueID;

            foreach (string mod in tmxPack.hasMods)
                if (!helper.ModRegistry.IsLoaded(mod))
                    return;

            foreach (string mod in tmxPack.hasNotMods)
                if (helper.ModRegistry.IsLoaded(mod))
                    return;

            if (tmxPack.scripts.Count > 0)
                foreach (string script in tmxPack.scripts)
                    PyLua.loadScriptFromFile(Path.Combine(pack.DirectoryPath, script), packModId);

            PyLua.loadScriptFromFile(Path.Combine(Helper.DirectoryPath, "sr.lua"), "Platonymous.TMXLoader.SpouseRoom");

            List<MapEdit> spouseRoomMaps = new List<MapEdit>();
            foreach (SpouseRoom room in tmxPack.spouseRooms)
            {
                if (room.tilefix && !Overrides.NPCs.Contains(room.name))
                    Overrides.NPCs.Add(room.name);

                if (room.file != "none")
                {
                    spouseRoomMaps.Add(new MapEdit() { info = room.name, name = "FarmHouse1_marriage", file = room.file, position = new[] { 29, 1 } });
                    spouseRoomMaps.Add(new MapEdit() { info = room.name, name = "FarmHouse2_marriage", file = room.file, position = new[] { 35, 10 } });
                    spouseRoomMaps.Add(new MapEdit() { info = room.name, name = "Cabin1_marriage", file = room.file, position = new[] { 29, 1 } });
                    spouseRoomMaps.Add(new MapEdit() { info = room.name, name = "Cabin2_marriage", file = room.file, position = new[] { 35, 10 } });
                }
            }

            foreach (MapEdit edit in spouseRoomMaps)
            {
                string filePath = Path.Combine(pack.DirectoryPath, edit.file);
                Map map = TMXContent.Load(edit.file, Helper, pack);
                if (map == null)
                    continue;
                edit._pack = pack;

                this.AssetEditors.Add(new TMXAssetEditor(packModId, edit, map, EditType.SpouseRoom));
                //  mapsToSync.AddOrReplace(edit.name, map);
            }

            foreach (TileShop shop in tmxPack.shops)
            {
                tileShops.Remove(shop);
                tileShops.Add(shop, shop.inventory);
                foreach (string path in shop.portraits)
                    pack.ModContent.Load<Texture2D>(path).inject(@"Portraits/" + Path.GetFileNameWithoutExtension(path));
            }

            foreach (NPCPlacement edit in tmxPack.festivalSpots)
            {
                festivals.Remove(edit.map);
                festivals.Add(edit.map);
                addAssetEditor(new TMXAssetEditor(packModId, edit, EditType.Festival));
                // mapsToSync.AddOrReplace(edit.map, original);
            }

            foreach (NPCPlacement edit in tmxPack.placeNPCs)
            {
                helper.Events.GameLoop.SaveLoaded += (s, e) =>
                {
                    if (Game1.getCharacterFromName(edit.name) == null)
                    {
                        Game1.locations.Where(gl => gl.Name == edit.map).First().addCharacter(new NPC(new AnimatedSprite("Characters\\" + edit.name, 0, 16, 32), new Vector2(edit.position[0], edit.position[1]), edit.map, 0, edit.name, edit.datable, Helper.GameContent.Load<Texture2D>($"Portraits/{edit.name}")));
                        //Game1.locations.Where(gl => gl.Name == edit.map).First().addCharacter(new NPC(new AnimatedSprite("Characters\\" + edit.name, 0, 16, 32), new Vector2(edit.position[0], edit.position[1]), edit.map, 0, edit.name, Helper.GameContent.Load<Texture2D>($"Portraits/{edit.name}"),true));
                    }
                };
            }

            foreach (MapEdit edit in tmxPack.addMaps)
            {
                Map map = TMXContent.Load(edit.file, Helper, pack);
                if (map == null)
                    continue;

                TMXAssetEditor.editWarps(map, edit.addWarps, edit.removeWarps, map);

                string groupName = "TMXL";

                if (packModId.Contains("StardewValleyExpanded"))
                    groupName = "SDV Expanded";

                if (edit.addLocation)
                {
                    if (!map.Properties.ContainsKey("Group"))
                        map.Properties.Add("Group", groupName);

                    if (!map.Properties.ContainsKey("Name"))
                        map.Properties.Add("Name", edit.name);
                }

                map.inject("Maps/" + edit.name);

                edit._map = map;
                edit._pack = pack;
                if (edit.addLocation)
                    addedLocations.Add(edit);
            }

            foreach (MapEdit edit in tmxPack.replaceMaps)
            {
                Map map = TMXContent.Load(edit.file, Helper, pack);
                if (map == null)
                    continue;
                edit._pack = pack;
                addAssetEditor(new TMXAssetEditor(packModId, edit, map, EditType.Replace));
                // mapsToSync.AddOrReplace(edit.name, map);
            }

            foreach (MapEdit edit in tmxPack.mergeMaps)
            {
                Map map = TMXContent.Load(edit.file, Helper, pack);
                if (map == null)
                    continue;
                edit._pack = pack;
                addAssetEditor(new TMXAssetEditor(packModId, edit, map, EditType.Merge));
                // mapsToSync.AddOrReplace(edit.name, map);
            }

            foreach (MapEdit edit in tmxPack.onlyWarps)
            {
                addAssetEditor(new TMXAssetEditor(packModId, edit, null, EditType.Warps));
                // mapsToSync.AddOrReplace(edit.name, map);
            }

            foreach (BuildableEdit edit in tmxPack.buildables)
            {
                foreach (var l in (LocalizedContentManager.LanguageCode[])Enum.GetValues(typeof(LocalizedContentManager.LanguageCode)))
                    if (!edit.translations.ContainsKey(l.ToString()))
                        edit.translations.Add(l.ToString(), BuildableTranslation.FromEdit(edit));

                // pack.WriteJsonFile<TMXContentPack>("content.json", tmxPack);

                edit._icon = pack.ModContent.Load<Texture2D>(edit.iconFile);
                edit._map = TMXContent.Load(edit.file, Helper, pack);
                if (edit._map == null)
                    continue;
                edit._pack = pack;
                buildables.Add(edit);
            }
        }

        private void loadContentPacks()
        {
            PyDraw.getRectangle(64, 64, Color.Transparent).inject(@"Portraits/PlayerShop");

            foreach (IContentPack pack in GetContentPacks())
                loadPack(pack,"content");

            contentPacksLoaded = true;
        }

        private void loadContentPacksLate()
        {
            foreach (TMXContentPack pack in latePacks)
                loadPack(pack);

            latePacks.Clear();
        }

        private TMXAssetEditor addAssetEditor(TMXAssetEditor editor)
        {
            if (editor.conditions != "" || editor.inLocation != null)
                conditionals.Add(editor);

            this.AssetEditors.Add(editor);
            return editor;
        }

        private TMXAssetEditor removeAssetEditor(TMXAssetEditor editor)
        {
            if (conditionals.Contains(editor))
                conditionals.Remove(editor);

            this.AssetEditors.Remove(editor);
            return editor;
        }

        private void exportAllMaps()
        {
            string exportFolderPath = Path.Combine(Helper.DirectoryPath, "Converter", "FullMapExport");
            if (Directory.Exists(exportFolderPath))
                Directory.CreateDirectory(exportFolderPath);

            string contentPath = Constants.ContentPath;

            if (contentPath == null || contentPath == "")
                return;

            string[] files = Directory.GetFiles(Path.Combine(contentPath, "Maps"), "*.xnb", SearchOption.TopDirectoryOnly);
            foreach (string file in files)
            {
                string fileName = new FileInfo(file).Name;
                string folderName = new FileInfo(file).Directory.Name;

                if (fileName[0] == fileName.ToLower()[0])
                    continue;

                Map map = null;
                string path = Path.Combine(folderName, fileName);

                try
                {
                    map = Helper.GameContent.Load<Map>(path);
                    map.LoadTileSheets(Game1.mapDisplayDevice);
                }
                catch (Exception ex)
                {
                    Monitor?.Log(ex.Message + ":" + ex.StackTrace);
                    continue;
                }

                if (map == null)
                    continue;

                string exportPath = Path.Combine(exportFolderPath, fileName.Replace(".xnb", ".tmx"));
                TMXContent.Save(map, exportPath, true, Monitor);
            }
        }

        private bool showAll = false;
        private string set = "All";

        private void showBuildablesMenu(int position = 0, string selected = "none", bool remove = false, SaveBuildable selectedToRemove = null)
        {
            if (set == "All")
                set = i18n.Get("All");

            if (!Game1.IsMasterGame)
                return;

            if (buildables.Count == 0 || !Context.IsWorldReady || Game1.currentLocation.isStructure.Value)
                return;

            if (remove && buildablesBuild.Where(bb => bb.Location == Game1.currentLocation.Name).Count() < 1)
            {
                showBuildablesMenu();
                return;
            }

            BuildableEdit edit = null;
            edit = buildables.Find(bd => bd.id == selected);

           // Dictionary<string, string> colors = new Dictionary<string, string>();

            UIElement overlay = null;
            if (edit is BuildableEdit)
            {
                edit._map = TMXContent.Load(edit.file, Helper, edit._pack);

                if (edit._map == null)
                    return;
            }
            int menuWidth = 280;
            int menuHeight = 440;

            UIElement container = UIElement.GetContainer("BuildablesMenuContainer", 0, UIHelper.GetBottomRight(-64, -32, menuWidth, menuHeight)).WithInteractivity(draw: (b, e) =>
            {
                if (selected != "none" && !remove && overlay != null)
                {
                    Point pos = new Point(Game1.getMouseX(), Game1.getMouseY());

                    if (e.Bounds.Contains(pos))
                        return;


                    if (edit is BuildableEdit bb && bb._map is Map bmap)
                    {
                        var o = overlay.Bounds;
                        xTile.Dimensions.Rectangle v = new xTile.Dimensions.Rectangle(0, 0, Game1.viewport.Width, Game1.viewport.Height);
                        pos = new Point(o.X, o.Y);

                        foreach (xTile.Layers.Layer layer in bmap.Layers)
                        {
                            layer.Properties["tempOffsetx"] = pos.X;
                            layer.Properties["tempOffsety"] = pos.Y;

                            if (layer.Properties.ContainsKey("isImageLayer"))
                            {
                                Point buildPosition = new Point(pos.X + Game1.viewport.X, pos.Y + Game1.viewport.Y);

                                if (layer.Properties.ContainsKey("offsetx"))
                                    layer.Properties["tempOffsetx"] = (int.Parse(layer.Properties["offsetx"].ToString()) + buildPosition.X).ToString();

                                if (layer.Properties.ContainsKey("offsety"))
                                    layer.Properties["tempOffsety"] = (int.Parse(layer.Properties["offsety"].ToString()) + buildPosition.Y).ToString();
                            }
                        }

                        foreach (xTile.Layers.Layer layer in bmap.Layers.Where(l => l.Properties.ContainsKey("DrawBefore") && l.Properties["DrawBefore"].ToString() == "Back"))
                            PyMaps.drawLayer(layer, Game1.mapDisplayDevice, v, 4, Location.Origin, false);

                        if (bmap.GetLayer("Back") is xTile.Layers.Layer backLayer)
                            PyMaps.drawLayer(backLayer, Game1.mapDisplayDevice, v, 4, Location.Origin, false);

                        foreach (xTile.Layers.Layer layer in bmap.Layers.Where(l => l.Properties.ContainsKey("Draw") && l.Properties["Draw"].ToString() == "Back"))
                            PyMaps.drawLayer(layer, Game1.mapDisplayDevice, v, 4, Location.Origin, false);

                        foreach (xTile.Layers.Layer layer in bmap.Layers.Where(l => l.Properties.ContainsKey("DrawAbove") && l.Properties["DrawAbove"].ToString() == "Back"))
                            PyMaps.drawLayer(layer, Game1.mapDisplayDevice, v, 4, Location.Origin, false);

                        foreach (xTile.Layers.Layer layer in bmap.Layers.Where(l => l.Properties.ContainsKey("DrawBefore") && l.Properties["DrawBefore"].ToString() == "Buildings"))
                            PyMaps.drawLayer(layer, Game1.mapDisplayDevice, v, 4, Location.Origin, false);

                        if (bmap.GetLayer("Buildings") is xTile.Layers.Layer buildingsLayer)
                            PyMaps.drawLayer(buildingsLayer, Game1.mapDisplayDevice, v, 4, Location.Origin, false);

                        foreach (xTile.Layers.Layer layer in bmap.Layers.Where(l => l.Properties.ContainsKey("Draw") && l.Properties["Draw"].ToString() == "Buildings"))
                            PyMaps.drawLayer(layer, Game1.mapDisplayDevice, v, 4, Location.Origin, false);

                        foreach (xTile.Layers.Layer layer in bmap.Layers.Where(l => l.Properties.ContainsKey("DrawAbove") && l.Properties["DrawAbove"].ToString() == "Buildings"))
                            PyMaps.drawLayer(layer, Game1.mapDisplayDevice, v, 4, Location.Origin, false);

                        foreach (xTile.Layers.Layer layer in bmap.Layers.Where(l => l.Properties.ContainsKey("DrawBefore") && l.Properties["DrawBefore"].ToString() == "Front"))
                            PyMaps.drawLayer(layer, Game1.mapDisplayDevice, v, 4, Location.Origin, false);

                        if (bmap.GetLayer("Front") is xTile.Layers.Layer frontLayer)
                            PyMaps.drawLayer(frontLayer, Game1.mapDisplayDevice, v, 4, Location.Origin, false);

                        foreach (xTile.Layers.Layer layer in bmap.Layers.Where(l => l.Properties.ContainsKey("Draw") && l.Properties["Draw"].ToString() == "Front"))
                            PyMaps.drawLayer(layer, Game1.mapDisplayDevice, v, 4, Location.Origin, false);

                        foreach (xTile.Layers.Layer layer in bmap.Layers.Where(l => l.Properties.ContainsKey("DrawAbove") && l.Properties["DrawAbove"].ToString() == "Front"))
                            PyMaps.drawLayer(layer, Game1.mapDisplayDevice, v, 4, Location.Origin, false);

                        foreach (xTile.Layers.Layer layer in bmap.Layers.Where(l => l.Properties.ContainsKey("DrawBefore") && l.Properties["DrawBefore"].ToString() == "AlwaysFront"))
                            PyMaps.drawLayer(layer, Game1.mapDisplayDevice, v, 4, Location.Origin, false);

                        if (bmap.GetLayer("AlwaysFront") is xTile.Layers.Layer alwaysfrontLayer)
                            PyMaps.drawLayer(alwaysfrontLayer, Game1.mapDisplayDevice, v, 4, Location.Origin, false);

                        foreach (xTile.Layers.Layer layer in bmap.Layers.Where(l => l.Properties.ContainsKey("Draw") && l.Properties["Draw"].ToString() == "AlwaysFront"))
                            PyMaps.drawLayer(layer, Game1.mapDisplayDevice, v, 4, Location.Origin, false);

                        foreach (xTile.Layers.Layer layer in bmap.Layers.Where(l => l.Properties.ContainsKey("DrawAbove") && l.Properties["DrawAbove"].ToString() == "AlwaysFront"))
                            PyMaps.drawLayer(layer, Game1.mapDisplayDevice, v, 4, Location.Origin, false);


                    }
                }
            });

            (UIHelper.BounceClose as AnimatedTexture2D).Paused = true;
            (UIHelper.BounceClose as AnimatedTexture2D).CurrentFrame = 0;
            (UIHelper.BounceClose as AnimatedTexture2D).SetSpeed(12);

            UIElement closeBtn = UIElement.GetImage(UIHelper.BounceClose, Color.White, "CloseBtn", 1, 9, UIHelper.GetTopRight(20, -40, 40)).WithInteractivity(click: (point, right, released, hold, element) =>
             {
                 if (released)
                     Game1.activeClickableMenu.exitThisMenu();
             }, hover: (point, hoverin, element) =>
              {
                 if (hoverin != element.WasHover)
                     Game1.playSound("smallSelect");

                 AnimatedTexture2D a = (element.Theme as AnimatedTexture2D);

                 if (hoverin)
                     a.Paused = false;
                 else
                 {
                     a.Paused = true;
                     a.CurrentFrame = 0;
                 }

             });
            container.Add(closeBtn);


            container.WithInteractivity(keys: (key, released, e) =>
             {
                 if (key == Microsoft.Xna.Framework.Input.Keys.F2)
                 {
                     showBuildablesMenu();
                     return;
                 }

                 if (key == Microsoft.Xna.Framework.Input.Keys.F3)
                 {
                     showBuildablesMenu(0, "none", !remove, null);
                     return;
                 }

                 if (key == Microsoft.Xna.Framework.Input.Keys.Back && buildablesBuild.Count > 0)
                 {
                     var last = buildablesBuild.Where(l => l.Location == Game1.currentLocation.Name).Last();

                     removeSavedBuildable(last,true,true);
                     showBuildablesMenu(position, selected, remove, selectedToRemove);
                     return;
                 }

                 if (key == Microsoft.Xna.Framework.Input.Keys.Delete)
                     if (!remove)
                         showBuildablesMenu(position);
                     else if (selectedToRemove != null)
                     {
                         var toRemove = selectedToRemove;

                         removeSavedBuildable(toRemove,true,true);
                         showBuildablesMenu(position, selected, remove, null);
                     }
             }
            );

            UIElement pickBuild = UIElement.GetImage(UIHelper.TabTheme, Color.White, "buildTab", (remove ? 0.6f : 0.9f), -1, UIHelper.GetTopLeft(20, -40, 120, 60)).AsTiledBox(16, true).WithInteractivity(hover: (point, hoverin, element) => {
                if (remove)
                {
                    (element.GetElementById("buildText") as UITextElement).TextColor = hoverin ? Color.Cyan * 0.8f : Color.Black * 0.8f;

                    if (hoverin != element.WasHover)
                        Game1.playSound("smallSelect");
                }
            }, click: (point, right, release, hold, element) => {

                if (release && !right && remove)
                    showBuildablesMenu();
            });
            UITextElement textBuild = new UITextElement(i18n.Get("Build"), Game1.smallFont, Color.Black * 0.8f, 0.5f, 1, "buildText", 0, UIHelper.GetCentered());
            pickBuild.Add(textBuild);
            container.Add(pickBuild);

            UIElement pickRemove = UIElement.GetImage(UIHelper.TabTheme, Color.White, "removeTab", (!remove ? 0.6f : 9f), -1, UIHelper.GetTopLeft(160, -40, 120, 60)).AsTiledBox(16, true).WithInteractivity(hover: (point, hoverin, element) => {
                if (!remove)
                {
                    (element.GetElementById("removeText") as UITextElement).TextColor = hoverin ? Color.Orange * 0.8f : Color.Black * 0.8f;

                    if (hoverin != element.WasHover)
                        Game1.playSound("smallSelect");
                }
            }, click: (point, right, release, hold, element) => {

                if (release && !right && !remove)
                    showBuildablesMenu(remove: true);
            }); ;
            UITextElement textRemove = new UITextElement(i18n.Get("Remove"), Game1.smallFont, Color.Black * 0.8f, 0.5f, 1, "removeText", 0, UIHelper.GetCentered());
            pickRemove.Add(textRemove);
            container.Add(pickRemove);

            UIElement back = UIElement.GetImage(UIHelper.DarkTheme, Color.White * 0.9f, "BMCBack").AsTiledBox(16, true);
            container.Add(back);
            UIElement listContainer = UIElement.GetContainer("BMCListContainer", 0, UIHelper.GetCentered(0, 0, menuWidth - 30, menuHeight - 20));
            back.Add(listContainer);
            UIElementList list = new UIElementList("BMCList", startPosition: position, margin: 5, elementPositioner: UIHelper.GetFixed(0, 0, 1f, (menuHeight - 40) /4));
            listContainer.Add(list);
            Texture2D entryBack = PyDraw.getBorderedRectangle(18, 18, Color.White * 0.1f, 4, Color.White * 0.5f);
            Texture2D entryBackHover = PyDraw.getBorderedRectangle(18, 18, Color.White * 0.1f, 4, Color.LightCyan);
            Texture2D entryBackSelected = PyDraw.getBorderedRectangle(18, 18, Color.White * 0.1f, 4, Color.Orange);
            Texture2D entryBackSelectedDelete = PyDraw.getBorderedRectangle(18, 18, Color.White * 0.1f, 4, Color.Red);

            UIElement fullContainer = UIElement.GetContainer("BMView");

            if (selected != null && !remove && edit is BuildableEdit be && be._map is Map map)
            {
                UIElement colorPicker = null;

                Texture2D pattern = PyDraw.getPattern(map.DisplayWidth, map.DisplayHeight, PyDraw.getRectangle(32, 32, Color.White * 0.2f), PyDraw.getRectangle(32, 32, Color.White * 0.4f));
                overlay = UIElement.GetImage(pattern, Color.Cyan, "BMPattern", 1, -1, (t, p) =>
                {
                    if (colorPicker != null && colorPicker.Bounds.Contains(Game1.getMousePosition()))
                        return new Microsoft.Xna.Framework.Rectangle(50, 50, pattern.Width, pattern.Height);


                    Point pt = Game1.GlobalToLocal((Game1.currentLocation.getTileAtMousePosition() * Game1.tileSize)).toPoint();
                    return new Microsoft.Xna.Framework.Rectangle(pt.X, pt.Y, pattern.Width, pattern.Height);
                }).WithInteractivity(update: (t, e) =>
                {
                    Point pos = new Point(Game1.getMouseX(), Game1.getMouseY());

                    e.Visible = !container.Bounds.Contains(pos);

                    e.UpdateBounds();
                }, click: (point, right, release, hold, element) =>
                {
                    if (selected == "none" || edit == null || listContainer.Bounds.Contains(Game1.getMousePosition()) || (colorPicker != null && colorPicker.Bounds.Contains(Game1.getMousePosition())))
                        return;

                    if (right)
                    {
                        showBuildablesMenu(list.Position);
                        return;
                    }
                    if (release)
                    {
                        try
                        {
                            Dictionary<string, string> colors = new Dictionary<string, string>();
                            foreach (xTile.Layers.Layer layer in map.Layers)
                            {
                                try
                                {
                                    if (layer.Properties.ContainsKey("tempOffsetx"))
                                        layer.Properties.Remove("tempOffsetx");

                                    if (layer.Properties.ContainsKey("tempOffsety"))
                                        layer.Properties.Remove("tempOffsety");

                                    if (layer.Properties.ContainsKey("ColorId") && layer.Properties.ContainsKey("Color"))
                                    {
                                        colors.Remove(layer.Properties["ColorId"].ToString());
                                        colors.Add(layer.Properties["ColorId"].ToString(), layer.Properties["Color"].ToString());
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Monitor?.Log(ex.Message + ":" + ex.StackTrace, LogLevel.Error);

                                }
                            }

                            Point pos = Game1.currentLocation.getTileAtMousePosition().toPoint();

                            buildBuildableEdit(true, edit, Game1.currentLocation, pos, colors);

                            Game1.playSound("drumkit0");

                            showBuildablesMenu(list.Position);
                        }
                        catch(Exception ex)
                        {
                            Monitor?.Log(ex.Message + ":" + ex.StackTrace, LogLevel.Error);
                        }
                    }

                });
                try
                {
                    List<string> cLayers = new List<string>();
                    List<Color> cColors = new List<Color>();
                    foreach (xTile.Layers.Layer layer in map.Layers)
                        if (layer.Properties.ContainsKey("Color") && layer.Properties.ContainsKey("ColorId"))
                        {
                            string[] c = layer.Properties["Color"].ToString().Split(' ');
                                if (!cLayers.Contains(layer.Properties["ColorId"].ToString()))
                                {
                                    cLayers.Add(layer.Properties["ColorId"].ToString());
                                    cColors.Add(new Color(int.Parse(c[0]), int.Parse(c[1]), int.Parse(c[2]), c.Length > 3 ? int.Parse(c[3]) : 255));
                                }
                            
                        }

                    if (cColors.Count > 0)
                    {
                        colorPicker = UIPresets.GetColorPicker(cColors, (index, color) =>
                        {
                            foreach (xTile.Layers.Layer l in map.Layers.Where(ly => ly.Properties != null && ly.Properties.ContainsKey("ColorId") && ly.Properties.ContainsKey("Color") && ly.Properties["ColorId"] == cLayers[index]))
                                l.Properties["Color"] = color.R + " " + color.G + " " + color.B + " " + color.A;
                        }, true);
                        colorPicker.Positioner = UIHelper.GetBottomLeft(10, -10, colorPicker.Bounds.Width + 20, colorPicker.Bounds.Height + 20);
                        colorPicker.Z = 99;
                        fullContainer.Add(colorPicker);
                    }
                }
                catch (Exception e)
                {
                    Monitor?.Log(e.Message + ":" + e.StackTrace, LogLevel.Error);
                }
            }

            UIElement listDown = UIElement.GetImage(UIHelper.ArrowDown, Color.White, "Down", 0.5f, 1, UIHelper.GetBottomLeft(-48, 0, 32)).WithInteractivity(hover:(point, hoverin,element) =>
            {
                element.Opacity = hoverin ? 1f : 0.5f;

                if (hoverin != element.WasHover)
                    Game1.playSound("smallSelect");
            }, click:(point, right,release,hold,element) =>
            {
                if (release)
                {
                    Game1.playSound("smallSelect");
                    list.NextPosition();
                }
            });

            UIElement listUp = UIElement.GetImage(UIHelper.ArrowUp, Color.White, "Up", 0.5f, 1, UIHelper.GetBottomLeft(-48, -64, 32)).WithInteractivity(hover: (point, hoverin, element) =>
            {
                element.Opacity = hoverin ? 1f : 0.5f;

                if (hoverin != element.WasHover)
                    Game1.playSound("smallSelect");
            }, click: (point, right, release, hold, element) =>
            {
                if (release)
                {
                    Game1.playSound("smallSelect");
                    list.PreviousPosition();
                }
            }); ;

            back.Add(listDown);
            back.Add(listUp);


            if (overlay != null)
                fullContainer.Add(overlay);

            fullContainer.Add(container);

            if (remove)
            {
                var rlist = new List<SaveBuildable>(buildablesBuild.Where(bbe =>bbe.Location == Game1.currentLocation.Name || showAll));
                rlist.Reverse();

                foreach (var bb in rlist)
                {
                    var b = buildables.Find(bd => bd.id == bb.Id);
                    if (b == null || b.set == "Hidden")
                        continue;

                    if (set != i18n.Get("All") && b.translations[helper.Translation.LocaleEnum.ToString()].getSetName() != set)
                        continue;

                    UIElement buildableEntry = UIElement.GetImage(selectedToRemove == bb ? entryBackSelected : entryBack, Color.White, b.id, 1f).AsTiledBox(6, true).WithInteractivity(hover: (p, hoverin, element) =>
                    {
                        if (selectedToRemove != bb)
                            element.Theme = hoverin ? entryBackHover : entryBack;
                        else
                            element.Theme = hoverin ? entryBackSelectedDelete : entryBackSelected;

                        if (hoverin != element.WasHover)
                            Game1.playSound("smallSelect");

                    }, click: (p, right, release, hold, element) =>
                    {
                        if(release && !right && selectedToRemove == bb)
                        {
                            var toRemove = selectedToRemove;

                            removeSavedBuildable(toRemove,true,true);
                            showBuildablesMenu(position, selected, remove, null);
                        }
                        else if (release && !right)
                            showBuildablesMenu(list.Position,"none", true,bb);
                        else if (right)
                            showBuildablesMenu(list.Position, "none", true, null);
                    });
                    list.Add(buildableEntry);
                    UIElement buildableImage = UIElement.GetImage(b._icon, Color.White, b.id + "_icon", positioner: UIHelper.GetTopLeft(10, 0.1f, 0, 0.8f));
                    buildableEntry.Add(buildableImage);

                    UITextElement buildablePrice = new UITextElement("X " + bb.Position[0] + "  Y " + bb.Position[1], Game1.smallFont,  Color.White, 0.5f, 1, b.id + "_position", positioner: UIHelper.GetBottomRight(-10, -10));
                    buildableEntry.Add(buildablePrice);

                    UITextElement buildableName = new UITextElement(b.translations[helper.Translation.LocaleEnum.ToString()].name, Game1.smallFont, Color.White, 0.7f, 1, b.id + "_name", positioner: UIHelper.GetBottomRight(-10, -40));
                    buildableEntry.Add(buildableName);
                }
            }
            if (!remove)
            {
                List<string> sets = new List<string>() { i18n.Get("All") };
                Dictionary<string, int> bSets = new Dictionary<string, int>();
                int cBd = 0;
                foreach (var b in buildables.Where(c => c.set != "Hidden" && PyUtils.checkEventConditions(c.conditions, Game1.currentLocation)))
                {
                    if (!bSets.ContainsKey(b.translations[helper.Translation.LocaleEnum.ToString()].getSetName()))
                        bSets.Add(b.translations[helper.Translation.LocaleEnum.ToString()].getSetName(), 0);

                    bSets[b.translations[helper.Translation.LocaleEnum.ToString()].getSetName()]++;
                    cBd++;
                    if ((set != i18n.Get("All") && set != b.translations[helper.Translation.LocaleEnum.ToString()].getSetName()))
                        continue;

                    bool affordable = Game1.player.Money >= b.price;
                    bool buildable = true;

                    string itemString = "";

                    foreach(TileShopItem tItem in b.buildItems)
                    {
                        Item item = TMXActions.getItem(tItem.type, tItem.index, tItem.name);
                        if (item == null || !(item is StardewValley.Object))
                            continue;

                        if ((item as StardewValley.Object).bigCraftable.Value)
                            continue;

                        itemString += "  " + tItem.stock + " " + item?.Name;
                        
                        if (item != null && Game1.player.Items.CountId(tItem.index) < tItem.stock)
                            buildable = false;
                    }

                    UIElement buildableEntry = UIElement.GetImage(b.id == selected ? entryBackSelected : entryBack, Color.White, b.id, affordable && buildable ? 1f : 0.7f).AsTiledBox(6, true).WithInteractivity(hover: (p, hoverin, element) =>
                    {
                        if (element.Id != selected)
                            element.Theme = hoverin ? entryBackHover : entryBack;

                        if (hoverin != element.WasHover)
                            Game1.playSound("smallSelect");

                    }, click: (p, right, release, hold, element) =>
                    {
                        try
                        {
                            if (release && !right && element.Opacity == 1f)
                                showBuildablesMenu(list.Position, element.Id, false);
                        }
                        catch(Exception e)
                        {
                            Monitor?.Log(e.Message + ":" + e.StackTrace, LogLevel.Error);
                        }
                    });
                    list.Add(buildableEntry);
                    UIElement buildableImage = UIElement.GetImage(b._icon, Color.White, b.id + "_icon", positioner: UIHelper.GetTopLeft(10, 0.1f, 0, 0.8f));
                    buildableEntry.Add(buildableImage);

                    UITextElement buildableItems = new UITextElement((itemString == "" ? " ---" : itemString), Game1.smallFont, buildable ? Color.White : Color.Red, 0.5f, 1, b.id + "_price", positioner: UIHelper.GetBottomRight(-10, -10));
                    buildableEntry.Add(buildableItems);

                    UITextElement buildablePrice = new UITextElement(b.price + "g", Game1.smallFont, affordable ? Color.White : Color.Red, 0.5f, 1, b.id + "_price", positioner: UIHelper.GetBottomRight(-10, -40));
                    buildableEntry.Add(buildablePrice);

                    UITextElement buildableName = new UITextElement(b.translations[helper.Translation.LocaleEnum.ToString()].name, Game1.smallFont, Color.White, 0.7f, 1, b.id + "_name", positioner: UIHelper.GetBottomRight(-10, -60));
                    buildableEntry.Add(buildableName);
                }

                int sti = 0;

                foreach (string sKey in bSets.Keys.OrderByDescending(sk => bSets[sk]))
                    if (sets.Count < 12)
                        sets.Add(sKey);
                    else
                        break;

                bSets.Add(i18n.Get("All"), cBd);

                foreach(string st in sets)
                {
                    UIElement stBtnText = new UITextElement(st + " [" + bSets[st] +"]", Game1.smallFont, Color.Black, 0.5f,1, st + "_Text", 0, UIHelper.GetCentered());
                    int stW = (int) Math.Ceiling((stBtnText.Bounds.Width / 5f)) * 5;
                    int stH = (int)Math.Ceiling((stBtnText.Bounds.Height / 5f)) * 5;

                    UIElement cSetBtn = UIElement.GetImage(UIHelper.YellowbBoxTheme, Color.White, st, set == st ? 1 : 0.5f, 0, UIHelper.GetTopLeft(-(stW + 20), (stH + 15) * sti, (stW + 10), (stH + 10))).AsTiledBox(5,true).WithInteractivity(click:(point,right,release,hold,element)=>
                    {
                        if(release)
                        {
                            set = element.Id;
                            showBuildablesMenu(position, selected, remove, selectedToRemove);
                        }
                    });
                    cSetBtn.Add(stBtnText);
                    listContainer.Add(cSetBtn);

                    sti++;
                }
            }

            fullContainer.UpdateBounds();
            Game1.activeClickableMenu = new PlatoUIMenu("BuildablesMenu", fullContainer);
        }

    }
}

