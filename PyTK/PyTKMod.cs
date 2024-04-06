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
using HarmonyLib;
using System.Reflection;
using StardewValley.Menus;
using System.Collections.Generic;
using System.Linq;
using PyTK.Lua;
using xTile;
using PyTK.Overrides;
using PyTK.APIs;
using System.Threading.Tasks;
using System.Collections;
using System.Xml.Serialization;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using TMXTile;
using xTile.Layers;
using xTile.Tiles;
using StardewValley.Objects;
using PyTK.Events;

namespace PyTK
{
    public class PyTKConfig
    {
        public string[] Options { get; set; } = new string[0];
    }

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
        public static Dictionary<IManifest, Func<object, object>> PreSerializer = new Dictionary<IManifest, Func<object, object>>();
        public static Dictionary<IManifest, Func<object, object>> PostSerializer = new Dictionary<IManifest, Func<object, object>>();
        public static List<IInterceptor> ContentInterceptors = new List<IInterceptor>();

        internal static List<GameLocation> RecheckedLocations = new List<GameLocation>();

        internal static List<Type> SerializerTypes = new List<Type>();

        internal static object waitForIt = new object();
        internal static object waitForPatching = new object();
        internal static object waitForItems = new object();

        internal static Harmony hInstance;

        internal static UpdateTickedEventArgs updateTicked;

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

        internal static PyTKConfig Config { get; set; } = new PyTKConfig();

        public override void Entry(IModHelper helper)
        {
            _instance = this;

            try
            {
                Config = helper.ReadConfig<PyTKConfig>();
            }
            catch
            {
                Config = new PyTKConfig();
                helper.WriteConfig(Config);
            }
            hInstance = new Harmony("Platonymous.PyTK.Rev");
            helper.Events.Display.RenderingWorld += (s,e) =>
            {
                if (Game1.currentLocation is GameLocation location && location.Map is Map map && map.GetBackgroundColor() is TMXColor tmxColor)
                    Game1.graphics.GraphicsDevice.Clear(tmxColor.toColor());
            };

            PostSerializer.Add(ModManifest, Rebuilder);
            PreSerializer.Add(ModManifest, Replacer);

            harmonyFix();
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
                if(!(Game1.mapDisplayDevice is PyDisplayDevice))
                    Game1.mapDisplayDevice = hasMapTK ? Game1.mapDisplayDevice : PyDisplayDevice.Instance ?? new PyDisplayDevice(Game1.content, Game1.graphics.GraphicsDevice, adjustForCompat2);
            };

            helper.Events.GameLoop.SaveCreated += (s, e) =>
            {
                if (!(Game1.mapDisplayDevice is PyDisplayDevice))
                    Game1.mapDisplayDevice = hasMapTK ? Game1.mapDisplayDevice : PyDisplayDevice.Instance ?? new PyDisplayDevice(Game1.content, Game1.graphics.GraphicsDevice, adjustForCompat2);
            };

            helper.ConsoleCommands.Add("show_mapdata", "", (s, p) =>
             {
                 ShowMapData(Game1.currentLocation.Map);
             }

            );

            helper.Events.GameLoop.DayStarted += (s, e) =>
            {
                if (ReInjectCustomObjects)
                {
                    ReInjectCustomObjects = false;
                    CustomObjectData.injector?.Invalidate();
                    CustomObjectData.injectorBig?.Invalidate();
                }
            };

            helper.Events.GameLoop.UpdateTicked += (s, e) =>
            {
                updateTicked = e;
            };

            helper.Events.GameLoop.ReturnedToTitle += (s, e) =>
            {
                foreach(Layer l in PyMaps.LayerHandlerList.Keys)
                {
                    foreach (var h in PyMaps.LayerHandlerList[l])
                    {
                        l.AfterDraw -= h;
                        l.BeforeDraw -= h;
                    }
                }

                PyMaps.LayerHandlerList.Clear();
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
                if (Game1.IsMasterGame)
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
                CustomTVMod.reloadStrings();

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

            helper.Events.GameLoop.DayStarted += (s, e) =>
            {
                if(Game1.currentLocation is GameLocation loc)
                UpdateLuaTokens = true;
            };

            helper.Events.GameLoop.UpdateTicked += (s, e) => AnimatedTexture2D.ticked = e.Ticks;
        }

        private void ShowMapData(Map map)
        {
            Monitor.Log("Map Data for " + Game1.currentLocation.Name, LogLevel.Info);
            Monitor.Log("--------------------" + Game1.currentLocation.Name, LogLevel.Info);

            Monitor.Log("Layers: " + map.Layers.Count, LogLevel.Info);
            foreach (Layer l in map.Layers)
            {
                Monitor.Log("------- " + l.Id + " -------", LogLevel.Info);
                List<string> tileData = new List<string>();

                for (int x = 0; x < l.DisplayWidth / Game1.tileSize; x++)
                    for (int y = 0; y < l.DisplayHeight / Game1.tileSize; y++)
                    {
                        if (l.Tiles[x, y] is Tile tile)
                            Monitor.Log("[" + x + "," + y + "] => " + tile.TileSheet.Id + ":" + tile.TileIndex, LogLevel.Info);
                    }

            }

            Monitor.Log("--------------------" + Game1.currentLocation.Name, LogLevel.Info);

            Monitor.Log("Tilesheets: " + map.TileSheets.Count, LogLevel.Info);
            foreach (TileSheet ts in map.TileSheets)
            {
                Monitor.Log("[" + ts.Id + "] => " + ts.ImageSource + " (" + (Game1.mapDisplayDevice as PyDisplayDevice).GetTexture(ts)?.Width + "x"+ (Game1.mapDisplayDevice as PyDisplayDevice).GetTexture(ts)?.Height+ ")", LogLevel.Info);
            }
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
        
        public static Harmony instance = new Harmony("Platonymous.PyTK");

        private void harmonyFix()
        {
            OvSpritebatchNew.initializePatch(instance);

            instance.PatchAll(Assembly.GetExecutingAssembly());
            
            instance.Patch(typeof(SaveGame).GetMethod("Load", BindingFlags.Static | BindingFlags.Public), prefix: new HarmonyMethod(typeof(PyTKMod).GetMethod("saveLoadedXMLFix", BindingFlags.Static | BindingFlags.Public)));

            try
            {
                instance.Patch(AccessTools.Method(typeof(GameLocation), nameof(GameLocation.answerDialogue)), new HarmonyMethod(this.GetType(), nameof(AnswerDialoguePrefix)));
            }
            catch
            {

            }

            PatchGeneratedSerializers(new Assembly[] { Assembly.GetExecutingAssembly() });

            foreach (ConstructorInfo mc in typeof(GameLocation).GetConstructors())
                instance.Patch(mc, postfix: new HarmonyMethod(typeof(OvLocations).GetMethod("GameLocationConstructor", BindingFlags.Static | BindingFlags.Public)));

            Helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;

            if (Constants.TargetPlatform != GamePlatform.Android)
            {
                try
                {
                    SetUpAssemblyPatch(hInstance, new XmlSerializer[] { SaveGame.farmerSerializer, SaveGame.locationSerializer, SaveGame.serializer });
                }
                catch
                {
                    Monitor.Log("Failed to Patch: Serializers Assembly. If the game crashes on save, this might be the cause.", LogLevel.Info);
                }
            }

            Helper.Events.GameLoop.GameLaunched += (s, e) =>
            {
                if (Config.Options.Contains("DisableSerializerPatch"))
                    return;

                Task.Run(() =>
               {
                   lock (waitForIt)
                       PatchGeneratedSerializers(AppDomain.CurrentDomain.GetAssemblies().Where(a => a.FullName.Contains("Microsoft.GeneratedCode")));
               });
            };

            setupLoadIntercepter(instance);
        }

        public static void AnswerDialoguePrefix(GameLocation __instance, ref Response answer)
        {
            if (__instance.lastQuestionKey == null || answer == null || answer.responseKey == null)
                return;

            if (__instance.lastQuestionKey.ToLower() == "sleep" && answer.responseKey.ToLower() == "yes")
                PyTimeEvents.CallBeforeSleepEvents(null, new PyTimeEvents.EventArgsBeforeSleep(STime.CURRENT, false, ref answer));
        }

        private void setupLoadIntercepter(Harmony harmony)
        {

            if (!Config.Options.Contains("DisableFSPatch"))
            {
                int fsc = 0;

                    foreach (MethodBase m in typeof(Texture2D).GetMethods(BindingFlags.Public | BindingFlags.Static).Where(gm => gm.Name.Contains("FromStream") && gm.GetParameters().ToList().Exists(p => p.Name == "stream")))
                {
                    try
                    {
                        harmony.Patch(
                            original: m,
                            postfix: new HarmonyMethod(this.GetType().GetMethod("FromStreamIntercepter", BindingFlags.Public | BindingFlags.Static)));
                        fsc++;
                    }
                    catch
                    {
                        Monitor.Log("Failed to Patch: FromStream, this Error may not be critical", LogLevel.Info);
                    }
                }

            }


            try
            {
                harmony.Patch(
                original: AccessTools.Method(
                    Type.GetType("StardewModdingAPI.Framework.Content.AssetDataForImage, StardewModdingAPI"),
                    "PatchImage",
                    new[] { typeof(Texture2D), typeof(Rectangle), typeof(Rectangle), typeof(PatchMode) }
                ),
                prefix: new HarmonyMethod(this.GetType().GetMethod("PatchImage", BindingFlags.Public | BindingFlags.Static))
            );
            }
            catch
            {
                Monitor.Log("Failed to Patch: PatchImage, this Error may not be critical", LogLevel.Info);
            }


            try
            {
                harmony.Patch(
                   original: AccessTools.Method(Type.GetType("StardewModdingAPI.Framework.Content.AssetDataForImage, StardewModdingAPI"), "ExtendImage"),
                   prefix: new HarmonyMethod(this.GetType().GetMethod("ExtendImage", BindingFlags.Public | BindingFlags.Static))
               );
            }
            catch
            {
                Monitor.Log("Failed to Patch: ExtendImage, this Error may not be critical", LogLevel.Info);
            }

            Monitor.Log("Patching: FileStream Constructors", LogLevel.Trace);

            int fscc = 0;
                foreach (ConstructorInfo constructor in typeof(FileStream).GetConstructors().Where(c => c.GetParameters().ToList().Exists(p => p.ParameterType == typeof(string) && p.Name == "path")))
                {
                    try
                    {
                        harmony.Patch(
                       original: constructor,
                       prefix: new HarmonyMethod(this.GetType().GetMethod("FileStreamConstructorPre", BindingFlags.Public | BindingFlags.Static))
                        );
                    fscc++;
                    }
                    catch
                    {
                        Monitor.Log("Failed to Patch: FileStream Constructors, this Error may not be critical", LogLevel.Info);

                    }
                }
            Monitor.Log($"Patched: FileStream Constructors ({fscc})", LogLevel.Info);

            Monitor.Log("Patching: PremultiplyTransparency", LogLevel.Trace);

            try
            {
                harmony.Patch(
                    original: AccessTools.Method(Type.GetType("StardewModdingAPI.Framework.ContentManagers.ModContentManager, StardewModdingAPI"), "PremultiplyTransparency"),
                    prefix: new HarmonyMethod(this.GetType().GetMethod("PremultiplyTransparencyPre", BindingFlags.Public | BindingFlags.Static))
                );
            }
            catch
            {
                Monitor.Log("Failed to Patch: PremultiplyTransparency, this Error may not be critical", LogLevel.Info);
            }

            ContentInterceptors.Add(new TextureInterceptor<ScaleUpData>(ModManifest, ScaleUpInterceptor));
        }

        private static string openPath = "";

        public static void LoadAsset(string assetName, Type type, object __result)
        {
                _monitor.Log("Loading:" + assetName, LogLevel.Warn);
            
        }

        public static void PremultiplyTransparencyPre(object __instance, ref Texture2D texture)
        {
            if (openPath != "")
                FromPathIntercepter(openPath, ref texture);
        }

        public static void FileStreamConstructorPre(ref string path)
        {
            if (path.Contains(@"Maps/Maps") && !path.Contains("assets"))
                path = path.Replace(@"Maps/Maps", "Maps");

            openPath = path;
            
        }

        public static Texture2D ScaleUpInterceptor(Texture2D texture, ScaleUpData data, string path)
        {
            if (data is ScaleUpData && !(texture is ScaledTexture2D))
                {
                bool scaled = false, animated = false, loop = true;
                    float scale = 1f;
                    int tileWidth = 0, tileHeight = 0, fps = 0;

                    if (data.SourceArea is int[] area && area.Length == 4)
                        texture = texture.getArea(new Rectangle(area[0], area[1], area[2], area[3]));

                    if (data.Scale != 1f)
                    {
                        scale = data.Scale;
                        scaled = true;
                    }

                    if (data.Animation is Animation anim)
                    {
                        tileHeight = anim.FrameHeight == -1 ? texture.Height : Math.Min(texture.Height, anim.FrameHeight);
                        tileWidth = anim.FrameWidth == -1 ? texture.Width : Math.Min(texture.Width, anim.FrameWidth);
                        fps = anim.FPS;
                        loop = anim.Loop;

                        if (!(tileWidth == texture.Width && tileHeight == texture.Height))
                            animated = true;
                    }

                    if (animated)
                        return new AnimatedTexture2D(Premultiply(texture), tileWidth, tileHeight, fps, loop, !scaled ? 1f : scale);
                    else if (scaled)
                        return ScaledTexture2D.FromTexture(texture.ScaleUpTexture(1f / scale, false), Premultiply(texture), scale);
                }
                return texture;
        }

        public static Texture2D Premultiply(Texture2D texture)
        {
            Color[] data = new Color[texture.Width * texture.Height];
            texture.GetData(data);
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i].A == 0)
                    continue;

                data[i] = Color.FromNonPremultiplied(data[i].ToVector4());
            }

            texture.SetData(data);
            return texture;
        }


        public static bool ExtendImage(IAssetDataForImage __instance, int minWidth, int minHeight)
        {
            return !(__instance.Data is ScaledTexture2D || __instance.Data is MappedTexture2D);
        }
        public static bool PatchImage(IAssetDataForImage __instance, ref Texture2D source, ref Rectangle? sourceArea, Rectangle? targetArea, PatchMode patchMode)
        {
            var a = new Rectangle(0, 0, __instance.Data.Width, __instance.Data.Height);
            var s = new Rectangle(0, 0, source.Width, source.Height);
            var sr = !sourceArea.HasValue ? s : sourceArea.Value;
            var tr = !targetArea.HasValue ? sr : targetArea.Value;

            if (__instance.Data is ScaledTexture2D && !(source is ScaledTexture2D))
                return true;

            if (source is ScaledTexture2D scaled)
            {
                if (a == tr && patchMode == PatchMode.Replace)
                {
                    __instance.ReplaceWith(source);
                    return true;
                }

                if (patchMode == PatchMode.Overlay)
                    scaled.AsOverlay = true;

                if (scaled.AsOverlay)
                {
                    Color[] data = new Color[(int)(tr.Width) * (int)(tr.Height)];
                    __instance.Data.getArea(tr).GetData(data);
                    scaled.SetData<Color>(data);
                }
                
                if (__instance.Data is MappedTexture2D map)
                    map.Set(tr, source);
                else
                    __instance.ReplaceWith(new MappedTexture2D(__instance.Data, new Dictionary<Rectangle?, Texture2D>() { { tr, source } }));
            }
            else if (__instance.Data is MappedTexture2D map)
            {
                map.Set(tr, (sr.Width != source.Width || sr.Height != source.Height) ? source.getArea(sr) : source);
                return false;
            }else if(__instance.Data is ScaledTexture2D sc)
            {


                __instance.ReplaceWith(new MappedTexture2D(__instance.Data, new Dictionary<Rectangle?, Texture2D>() { { tr, (sr.Width != source.Width || sr.Height != source.Height) ? source.getArea(sr) : source } }));
                return false;
            }

            return true;
        }

        public static void FromStreamIntercepter(Stream stream, ref Texture2D __result)
        {
            if (stream is FileStream fs)
                FromPathIntercepter(fs.Name, ref __result);
        }

        public static void FromPathIntercepter(string path, ref Texture2D __result)
        {
            openPath = "";
            if (Path.GetFileNameWithoutExtension(path) is string key
                && Path.GetDirectoryName(path) is string dir
                && Path.Combine(dir, key + ".pytk.json") is string pytkDataFile
                && File.Exists(pytkDataFile)
                && Newtonsoft.Json.JsonConvert.DeserializeObject<InterceptorData>(File.ReadAllText(pytkDataFile)) is InterceptorData idata)
                foreach (IInterceptor<Texture2D> interceptor in ContentInterceptors
                .Where(i => i is IInterceptor<Texture2D>
                && i.DataType != null && idata.Mods.Contains(i.Mod.UniqueID)))
                    if (Newtonsoft.Json.JsonConvert.DeserializeObject(File.ReadAllText(pytkDataFile), interceptor.DataType) is object o && o.GetType() == interceptor.DataType)
                        __result = interceptor.Handler(__result, o, path);
        }

        public static bool saveWasLoaded = false;

        public static void saveLoadedXMLFix()
        {
            if (saveWasLoaded)
                return;

            lock (waitForIt)
            {
                saveWasLoaded = true;
            }
        }

        public void SetUpAssemblyPatch(Harmony instance, IEnumerable<XmlSerializer> serializers)
        {
            if (Config.Options.Contains("DisableSerializerPatch"))
                return;

            foreach (var serializer in serializers)
            {
                var cache = serializer.GetType().GetField("s_cache", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
                IDictionary cacheTable = (IDictionary)cache.GetType().GetField("_cache", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(cache);

                foreach (var c in cacheTable.Values)
                {
                    var a = (Assembly)c.GetType().GetField("_assembly", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(c);
                    PatchGeneratedSerializers(new Assembly[] { a });
                }

                instance.Patch(cache.GetType().GetMethod("Add", BindingFlags.NonPublic | BindingFlags.Instance), postfix: new HarmonyMethod(typeof(PyTKMod).GetMethod("AssemblyCachePatch", BindingFlags.Static | BindingFlags.Public)));
            }
        }

        public static void AssemblyCachePatch(string ns, object o, object assembly)
        {
            PatchGeneratedSerializers(new Assembly[] { (Assembly)assembly.GetType().GetField("_assembly", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(assembly) });
        }

        public static void serializeFix(ref System.Object o)
        {
            foreach (var serializer in PreSerializer.Keys)
                try
                {
                    o = PreSerializer[serializer].Invoke(o);
                }
                catch (Exception e)
                {
                    _monitor.Log("Error during serialization: " + serializer.Name, LogLevel.Error);
                    _monitor.Log(e.Message);
                    _monitor.Log(e.StackTrace);
                }
        }

        private void GameLoop_DayStarted(object sender, DayStartedEventArgs e)
        {
            Helper.Events.GameLoop.DayStarted -= GameLoop_DayStarted;
        }

        private static List<object> didPatch = new List<object>();
        public static void SerializersReinitialized( XmlSerializer serializer )
        {
            //_monitor.Log( "Serializer reinitialized; redoing patches: " + serializer );
            if ( serializer == null )
            {
                PatchGeneratedSerializers( new Assembly[] { Assembly.GetExecutingAssembly() } );
                _instance.SetUpAssemblyPatch( hInstance, new XmlSerializer[] { SaveGame.farmerSerializer, SaveGame.locationSerializer, SaveGame.serializer } );
                PatchGeneratedSerializers( AppDomain.CurrentDomain.GetAssemblies().Where( a => a.FullName.Contains( "Microsoft.GeneratedCode" ) ) );
            }
            else
            {
                _instance.SetUpAssemblyPatch( hInstance, new XmlSerializer[] { serializer } );
                PatchGeneratedSerializers( AppDomain.CurrentDomain.GetAssemblies().Where( a => a.FullName.Contains( "Microsoft.GeneratedCode" ) ) );
            }
        }

        public static void PatchGeneratedSerializers(IEnumerable<Assembly> assemblies)
        {
            foreach (var ass in assemblies)
                foreach (var ty in ass.GetTypes().Where(t => t.Name.StartsWith("XmlSerializer1") || t.Name.StartsWith("XmlSerializationWriter") || t.Name.StartsWith("XmlSerializationReader")))
                    PatchGeneratedSerializerType(ty);
        }

        public static void PatchGeneratedSerializerType(Type ty)
        {
            if ( didPatch.Contains( ty ) )
                return;
            didPatch.Add( ty );
            //_monitor.Log( "type:" + ty.FullName );

            if (ty.FullName.Contains("XmlSerializer1"))
            {
                if (Constants.TargetPlatform != GamePlatform.Android && ty.BaseType.GetField("s_cache", BindingFlags.NonPublic | BindingFlags.Static) is FieldInfo field && field.GetValue(null) is object cache)
                    if (cache.GetType().GetField("_cache", BindingFlags.NonPublic | BindingFlags.Instance) is FieldInfo cField && cField.GetValue(cache) is IDictionary cacheTable)
                    {
                        foreach (var c in cacheTable.Values)
                        {
                            var a = (Assembly)c.GetType().GetField("_assembly", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(c);
                            PatchGeneratedSerializers(new Assembly[] { a });
                        }

                        instance.Patch(cache.GetType().GetMethod("Add", BindingFlags.NonPublic | BindingFlags.Instance), postfix: new HarmonyMethod(typeof(PyTKMod).GetMethod("AssemblyCachePatch", BindingFlags.Static | BindingFlags.Public)));
                    }
            }
            else
            {
                foreach (var met in ty.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    string type = "--object";
                    try
                    {
                        if (met.Name.Contains("_") && (met.Name.StartsWith("Write") || met.Name.StartsWith("Read")))
                            if (met.Name.StartsWith("Write") && (new List<ParameterInfo>(met.GetParameters()).Exists(p => p.Name == "o" && p.ParameterType.IsClass && p.ParameterType.FullName.Contains("Stardew"))))
                            {
                                Type t = met.GetParameters().ToList().Where(p => p.Name == "o").Select(p => p.ParameterType).FirstOrDefault();
                                type = t.ToString();
                                hInstance.Patch(met, prefix: new HarmonyMethod(typeof(PyTKMod).GetMethod("saveXMLReplacer", BindingFlags.Static | BindingFlags.Public)));
                            }
                            else if (met.Name.StartsWith("Read") && met.ReturnType != null && met.ReturnType.IsClass && met.ReturnType.FullName.Contains("Stardew"))
                            {
                                Type t = met.ReturnType;
                                type = t.ToString();
                                hInstance.Patch(met, postfix: new HarmonyMethod(typeof(PyTKMod).GetMethod("saveXMLRebuilder", BindingFlags.Static | BindingFlags.Public)));
                            }
                    }
                    catch(Exception e) {
                        _monitor.Log(type, LogLevel.Error);
                        _monitor.Log(e.Message + ":" + e.StackTrace, LogLevel.Info);
                    }
                }
                        
            }
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
                  
                        _monitor.Log("Error during serialization: " + serializer.Name, LogLevel.Error);
                        _monitor.Log(e.Message);
                        _monitor.Log(e.StackTrace);
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
                    return Game1.getCharacterFromName(name) is NPC npcx && npcx.currentLocation == location && npcx.TilePoint.X == x;

                var y = int.Parse(v[2]);
                return Game1.getCharacterFromName(name) is NPC npc && npc.currentLocation == location && (x == -1 || npc.TilePoint.X == x) && (y == -1 || npc.TilePoint.Y == y);
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

        public static class GenericActionFactory
        {
            public static MethodInfo CreateMethodByParameter(Type parameterType, Action<object> action, bool result = false)
            {
                var createMethod = typeof(GenericActionFactory).GetMethod("CreateMethod")
                    .MakeGenericMethod(parameterType);

                var del = createMethod.Invoke(null, new object[] { action, result });

                return (MethodInfo) del;
            }
            
            public static MethodInfo CreateMethod<TEvent>(Action<object> action, bool result)
            {

                if(!result)
                return (new Action<TEvent>((o) => action.Invoke(o))).GetMethodInfo();
                else
                    return new Action<TEvent>((__result) => action.Invoke(__result)).Method;
            }
        }
    }
}
