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
using PyTK.Overrides;
using PyTK.APIs;
using System.Threading;
using System.IO;
using Microsoft.Xna.Framework.Graphics;

namespace PyTK
{

    public class PyTKMod : Mod
    {
        internal static IModEvents _events => _helper.Events;
        internal static bool _activeSpriteBatchFix = true;
        internal static string sdvContentFolder => PyUtils.ContentPath;
        internal static List<IPyResponder> responders;
        internal static PyTKSaveData saveData = new PyTKSaveData();

        internal static Dictionary<string, string> tokenStrings = new Dictionary<string, string>();
        internal static Dictionary<string, bool> tokenBoleans = new Dictionary<string, bool>();
        internal static bool UpdateCustomObjects = false;
        internal static bool ReInjectCustomObjects = false;
        internal static bool UpdateLuaTokens = false;
        internal static Dictionary<IManifest, Func<object, object>> PreSerializer = new Dictionary<IManifest, Func<object, object>>();
        internal static Dictionary<IManifest, Func<object, object>> PostSerializer = new Dictionary<IManifest, Func<object, object>>();
        internal static PyTKMod _instance { get; set; }
        internal static IMonitor _monitor {
            get {
                return _instance.Monitor;
            }
        }
        public static IModHelper _helper
        {
            get
            {
                return _instance.Helper;
            }
        }
        public override void Entry(IModHelper helper)
        {
            _instance = this;
            PostSerializer.Add(ModManifest, Rebuilder);
            PreSerializer.Add(ModManifest, Replacer);
            harmonyFix();

            FormatManager.Instance.RegisterMapFormat(new TMXTile.TMXFormat(Game1.tileSize / Game1.pixelZoom, Game1.tileSize / Game1.pixelZoom, Game1.pixelZoom, Game1.pixelZoom));
            
            initializeResponders();
            startResponder();
            registerConsoleCommands();
            CustomTVMod.load();
            PyLua.init();
            registerTileActions();
            registerEventPreconditions();
            SaveHandler.setUpEventHandlers();
            CustomObjectData.CODSyncer.start();
            ContentSync.ContentSyncHandler.initialize();

            helper.Events.GameLoop.DayStarted += (s, e) =>
            {
                if (ReInjectCustomObjects)
                {
                    ReInjectCustomObjects = false;
                    CustomObjectData.injector?.Invalidate();
                    CustomObjectData.injectorBig?.Invalidate();
                }
            };

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
                if (Game1.displayHUD && Context.IsWorldReady)
                PyTK.PlatoUI.UIHelper.DrawHud(e.SpriteBatch, true);

            };

            Helper.Events.Display.RenderedHud += (s, e) =>
            {
                if (Game1.displayHUD && Context.IsWorldReady)
                    PyTK.PlatoUI.UIHelper.DrawHud(e.SpriteBatch, false);
            };

            Helper.Events.Input.ButtonPressed += (s, e) =>
            {
                if (Game1.displayHUD && Context.IsWorldReady)
                {
                    if (e.Button == SButton.MouseLeft || e.Button == SButton.MouseRight)
                        PlatoUI.UIHelper.BaseHud.PerformClick(e.Cursor.ScreenPixels.toPoint(), e.Button == SButton.MouseRight, false, false);
                }
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


            helper.Events.GameLoop.GameLaunched += (s, e) =>
            {
                if (!Helper.ModRegistry.IsLoaded("spacechase0.GenericModConfigMenu"))
                    return;

                try
                {
                    registerCPTokens();
                }
                catch { }

            };

            helper.Events.GameLoop.DayStarted += (s, e) => UpdateLuaTokens = true;

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
            if (!e.IsLocalPlayer)
                return;

            e.NewLocation?.Map.enableMoreMapLayers();

            if (e.NewLocation is GameLocation g && g.map is Map m)
            {
                int forceX = Game1.player.getTileX();
                int forceY = Game1.player.getTileY();
                int forceF = Game1.player.FacingDirection;
                if (e.OldLocation is GameLocation og && m.Properties.ContainsKey("ForceEntry_" + og.Name)){
                    string[] pos = m.Properties["ForceEntry_" + og.Name].ToString().Split(' ');
                    if(pos.Length > 0 && pos[1] != "X")
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

        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            if (Game1.currentLocation is GameLocation g && g.map is Map m && m.Properties.ContainsKey("EntryAction"))
                TileAction.invokeCustomTileActions("EntryAction", g, Vector2.Zero, "Map");
        }
        
        public static HarmonyInstance instance = HarmonyInstance.Create("Platonymous.PyTK");

        private void harmonyFix()
        {
            OvSpritebatchNew.initializePatch(instance);
            // PyUtils.initOverride("SObject", PyUtils.getTypeSDV("Object"),typeof(DrawFix1), new List<string>() { "draw", "drawInMenu", "drawWhenHeld", "drawAsProp" });
            // PyUtils.initOverride("TemporaryAnimatedSprite", PyUtils.getTypeSDV("TemporaryAnimatedSprite"),typeof(DrawFix2), new List<string>() { "draw" });
            instance.PatchAll(Assembly.GetExecutingAssembly());

            instance.Patch(typeof(SaveGame).GetMethod("Load",BindingFlags.Static | BindingFlags.Public), prefix: new HarmonyMethod(typeof(PyTKMod).GetMethod("saveLoadedXMLFix", BindingFlags.Static | BindingFlags.Public)));
            GenerateSerializers();

            Helper.Events.GameLoop.DayStarted += (s, e) => saveWasLoaded = true;
            Helper.Events.GameLoop.ReturnedToTitle += (s, e) => saveWasLoaded = false;
            Helper.Events.GameLoop.DayStarted += GameLoop_DayStarted; 
                
        }

        private void GameLoop_DayStarted(object sender, DayStartedEventArgs e)
        {
            Thread thread = new Thread(PatchGeneratedSerializers);
            thread.Start();
            Helper.Events.GameLoop.DayStarted -= GameLoop_DayStarted;
        }

        public static bool serializerReady = false;
        public static bool saveWasLoaded = false;

        public static void saveLoadedXMLFix()
        {
            saveWasLoaded = true;
            while (!serializerReady)
                Thread.Sleep(10);
        }


        public static List<string> patchedMethods = new List<string>();
        public void GenerateSerializers()
        {
        Thread thread = new Thread(GenerateSerializersThread);
        thread.Start();
        }

        public void GenerateSerializersThread()
        {
            serializerReady = false;
            List<Type> AddedTypes = new List<Type>()
        {
                typeof(StardewValley.Object),
                typeof(StardewValley.Objects.Furniture),
                typeof(StardewValley.Item),
                 typeof(StardewValley.Tool),
                 typeof(StardewValley.Farm),
                 typeof(StardewValley.FarmAnimal),
                 typeof(StardewValley.Farmer),
                 typeof(StardewValley.AnimalHouse),
                 typeof(StardewValley.Character),
                 typeof(StardewValley.NPC),
                 typeof(StardewValley.GameLocation),
                 typeof(StardewValley.Locations.FarmHouse),
                 typeof(SaveGame)
        };

            List<string> AddedNamespaces = new List<string>()
            {
                ".Buildings",
                ".Characters",
                ".Locations",
                ".Objects",
                ".Monsters",
                ".Tools"
            }; 

            SaveGame sg = new SaveGame();

            foreach (Type type in AddedTypes)// typeof(Game1).Assembly.GetTypes().Where(t => t.GetConstructor(new Type[] { }) != null && !t.IsGenericType && t.IsPublic && t.IsClass && t.Namespace != null && t.Namespace.Contains("Stardew") && AddedNamespaces.Exists(n => t.Namespace.Contains(n))))
                SaveGame.GetSerializer(type);

            PatchGeneratedSerializers();
        }


        public static void PatchGeneratedSerializers()
        {
            foreach (var ass in AppDomain.CurrentDomain.GetAssemblies().Where(a => a.FullName.Contains("Microsoft.GeneratedCode")))
                foreach (var ty in ass.GetTypes().Where(t => t.Name.StartsWith("XmlSerializationWriter") || t.Name.StartsWith("XmlSerializationReader")))
                    PatchGeneratedSerializerType(ty);

            serializerReady = true;
        }

        public static void PatchGeneratedSerializerType(Type ty)
        {
            foreach (var met in ty.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance))
                   if (met.Name.Contains("_") && (met.Name.StartsWith("Write") || met.Name.StartsWith("Read")))
                       if (met.Name.StartsWith("Write") && (new List<ParameterInfo>(met.GetParameters()).Exists(p => p.Name == "o" && p.ParameterType.IsClass && p.ParameterType.FullName.Contains("Stardew"))))
                           instance.Patch(met, prefix: new HarmonyMethod(typeof(PyTKMod).GetMethod("saveXMLReplacer", BindingFlags.Static | BindingFlags.Public)));
                       else if (met.Name.StartsWith("Read") && met.ReturnType != null && met.ReturnType.IsClass && met.ReturnType.FullName.Contains("Stardew"))
                           instance.Patch(met, postfix: new HarmonyMethod(typeof(PyTKMod).GetMethod("saveXMLRebuilder", BindingFlags.Static | BindingFlags.Public)));
        }


        public static void saveXMLReplacer(ref object o)
        {
            foreach (var serializer in PreSerializer.Keys)
                try
                {
                    o = PreSerializer[serializer].Invoke(o);
                }
                catch(Exception e)
                {
                        _monitor.Log("Error during serialization: " + serializer.Name, LogLevel.Error);
                        _monitor.Log(e.Message);
                        _monitor.Log(e.StackTrace);
                }
        }

        public static void saveXMLRebuilder(ref object __result)
        {
            foreach (var serializer in PostSerializer.Keys)
                try
                {
                    __result = PostSerializer[serializer].Invoke(__result);
                }
                catch (Exception e)
                {
                    if (saveWasLoaded)
                    {
                        _monitor.Log("Error during serialization: " + serializer.Name, LogLevel.Error);
                        _monitor.Log(e.Message);
                        _monitor.Log(e.StackTrace);
                    }
                }
        }

        public object Rebuilder(object __result)
        {
            if (SaveHandler.isRebuildable(__result))
                return SaveHandler.rebuildElement(SaveHandler.getDataString(__result), __result);

            return __result;
        }

        public object Replacer(object o)
        {
            if (SaveHandler.hasSaveType(o))
                return SaveHandler.getReplacement(o);

            return o;
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
                return LuaUtils.switches(values.Replace("switch ",""));
            });

            PyUtils.addEventPrecondition("npcxy", (key, values, location) =>
            {
                var v = values.Split(' ');
                var name = v[0];

                if (v.Length == 1)
                    return Game1.getCharacterFromName(name) is NPC npcp && npcp.currentLocation == location;

                var x = int.Parse(v[1]);

                if (v.Length == 2)
                    return Game1.getCharacterFromName(name) is NPC npcx && npcx.currentLocation == location && npcx.getTileX() == x;

                var y = int.Parse(v[2]);
                return Game1.getCharacterFromName(name) is NPC npc && npc.currentLocation == location && (x == -1 || npc.getTileX() == x) && (y == -1 || npc.getTileY() == y);
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

                    foreach(Item item in items)
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
                return PyUtils.checkEventConditions(values.Replace("%div","/"), location, location);
            });


        }


        private void registerCPTokens()
        {
            if (!Helper.ModRegistry.IsLoaded("Pathoschild.ContentPatcher"))
                return;

            IContentPatcherAPI api = Helper.ModRegistry.GetApi<IContentPatcherAPI>("Pathoschild.ContentPatcher");
            /*
            api.RegisterToken(this.ModManifest, "LuaString", () =>
            {
                foreach (string k in tokenStrings.Keys)
                    if (tokenStrings[k] != PyUtils.getLuaString(k))
                        return true;

                return false;
            }, () => Context.IsWorldReady, (s) =>
            {
                tokenStrings.AddOrReplace(s, PyUtils.getLuaString(s));
                return new string[] { tokenStrings[s] };
            }, true, true);*/

            api.RegisterToken(this.ModManifest, "Conditional", () =>
            {
                foreach (string k in tokenBoleans.Keys)
                    if (tokenBoleans[k] != PyUtils.checkEventConditions(k.Split(new[] { " >: " }, StringSplitOptions.RemoveEmptyEntries)[0]))
                        return true;

                return false;
            }, () => Context.IsWorldReady, (s) =>
            {
                string[] parts = s.Split(new [] { " >: " },StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length < 2)
                    return null;

                tokenBoleans.AddOrReplace(s, PyUtils.checkEventConditions(parts[0]));
                return new string[] { tokenBoleans[s] ? parts[1] : parts.Length < 3 ? null : parts[2] };
            }, true, true);

            api.RegisterToken(
                mod: this.ModManifest,
                name: "ObjectByName",
                updateContext: () =>
                {
                    if (!PyTK.PyTKMod.UpdateCustomObjects)
                        return false;

                    UpdateCustomObjects = false;
                    return true;
                },
                isReady: () => Context.IsWorldReady,
                getValue: GetObjectByNameTokenValue,
                allowsInput: true,
                requiresInput: true
            );

            api.RegisterToken(
                mod: this.ModManifest,
                name: "LuaString",
                updateContext: () =>
                {
                    if (!UpdateLuaTokens)
                        return false;

                    UpdateLuaTokens = false;
                    return true;
                },
                isReady: () => Context.IsWorldReady,
                getValue: GetLuaString,
                allowsInput: true,
                requiresInput: true
            );
        }

        private IEnumerable<string> GetObjectByNameTokenValue(string input)
        {
            string[] request = input.Split(':');
            yield return
                (request.Length >= 2) ?
                PyTK.PyUtils.getItem(request[0], -1, request[1]) is StardewValley.Object obj ? obj.ParentSheetIndex.ToString() : "" :
                PyTK.PyUtils.getItem("Object", -1, request[0]) is StardewValley.Object obj2 ? obj2.ParentSheetIndex.ToString() : "";
        }

        private IEnumerable<string> GetLuaString(string input)
        {
            var script = PyLua.getNewScript();
            script.Globals["result"] = "";
                      script.DoString("result = (" + input + ")");
            yield return (string)script.Globals["result"];
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
                 Helper.ConsoleCommands.Trigger(key, action.Split(' '));
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

                        if(PyLua.hasScript(id))
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
