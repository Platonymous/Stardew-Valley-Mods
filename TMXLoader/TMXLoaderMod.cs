using Microsoft.Xna.Framework;
using PyTK;
using PyTK.Extensions;
using PyTK.Lua;
using PyTK.Tiled;
using PyTK.Types;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System.Collections.Generic;
using System.IO;
using xTile;
using Harmony;
using System.Reflection;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Locations;
using System.Xml;
using xTile.ObjectModel;
using System;

namespace TMXLoader
{
    public class TMXLoaderMod : Mod
    {
        internal static string contentFolder = "Maps";
        internal static IMonitor monitor;
        internal static IModHelper helper;
        internal static Dictionary<string, Map> mapsToSync = new Dictionary<string, Map>();
        internal static List<Farmer> syncedFarmers = new List<Farmer>();
        internal static Dictionary<string, List<TileShopItem>> tileShops = new Dictionary<string, List<TileShopItem>>();
        internal static Config config;
        internal static SaveData saveData = new SaveData();
        internal static List<MapEdit> addedLocations = new List<MapEdit>();
        internal static List<string> festivals = new List<string>();
        internal static List<TMXAssetEditor> conditionals = new List<TMXAssetEditor>(); 


        public override void Entry(IModHelper helper)
        {
            config = Helper.ReadConfig<Config>();
            TMXLoaderMod.helper = Helper;
            monitor = Monitor;


            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            // helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;

            helper.Events.Display.RenderingHud += (s, e) =>
            {
                PropertyValue rain = "";

                if (!(Game1.currentLocation is GameLocation location) || !(location.Map is Map map) || !map.Properties.TryGetValue("Raining", out rain))
                    return;

                if (Game1.isRaining && Game1.currentLocation.IsOutdoors && (!Game1.currentLocation.Name.Equals("Desert") && !(Game1.currentLocation is Summit)) && (!Game1.eventUp || Game1.currentLocation.isTileOnMap(new Vector2((float)(Game1.viewport.X / 64), (float)(Game1.viewport.Y / 64)))))
                    return;

                if (Game1.isRaining || rain == "Always")
                    for (int index = 0; index < Game1.rainDrops.Length; ++index)
                        Game1.spriteBatch.Draw(Game1.rainTexture, Game1.rainDrops[index].position, new Microsoft.Xna.Framework.Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.rainTexture, Game1.rainDrops[index].frame, -1, -1)), Color.White);
            };

            helper.Events.Player.Warped += Player_Warped;

            helper.Events.GameLoop.Saving += (s, e) =>
            {
                if (!Game1.IsMasterGame)
                    return;

                saveData = new SaveData();
                saveData.Locations = new List<SaveLocation>();

                foreach (var l in addedLocations)
                {
                    if (Game1.getLocationFromName(l.name) is GameLocation location)
                    {
                        PyTK.CustomElementHandler.SaveHandler.ReplaceAll(location, Game1.locations);
                        string objects = "";

                        XmlWriterSettings settings = new XmlWriterSettings();
                        settings.ConformanceLevel = ConformanceLevel.Auto;
                        settings.CloseOutput = true;

                        StringWriter objWriter = new StringWriter();
                        using (var writer = XmlWriter.Create(objWriter, settings))
                            SaveGame.locationSerializer.Serialize(writer, location);

                        objects = objWriter.ToString();

                        saveData.Locations.Add(new SaveLocation(location.Name, objects));
                    }
                }

                Helper.Data.WriteSaveData<SaveData>("Locations", saveData);
            };

            helper.Events.GameLoop.SaveLoaded += (s, e) =>
            {
                foreach (var location in addedLocations)
                {
                    if (Game1.getLocationFromName(location.name) == null)
                    {
                        GameLocation l = addLocation(location);
                        l.updateSeasonalTileSheets();
                    }
                }

                if (!Game1.IsMasterGame)
                    return;

                XmlReaderSettings settings = new XmlReaderSettings();
                settings.ConformanceLevel = ConformanceLevel.Auto;

                saveData = Helper.Data.ReadSaveData<SaveData>("Locations");
                if (saveData != null)
                    foreach (SaveLocation loc in saveData.Locations)
                    {
                        var inGame = Game1.getLocationFromName(loc.Name);

                        if (inGame == null)
                            continue;

                        StringReader objReader = new StringReader(loc.Objects);
                        GameLocation saved;
                        using (var reader = XmlReader.Create(objReader, settings))
                            saved = (GameLocation) SaveGame.locationSerializer.Deserialize(reader);

                        if(saved != null)
                        {
                            inGame.Objects.Clear();
                            if (saved.Objects.Count() > 0)
                                foreach (var obj in saved.Objects.Keys)
                                    inGame.Objects.Add(obj,saved.Objects[obj]);

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
                        }

                        PyTK.CustomElementHandler.SaveHandler.RebuildAll(inGame, Game1.locations);
                        inGame.DayUpdate(Game1.dayOfMonth);
                    }
            };
        }

        private string getAssetNameWithoutFolder(string asset)
        {
            return Path.GetFileNameWithoutExtension(asset).Split('/').Last().Split('\\').Last();
        }

        private void Player_Warped(object sender, WarpedEventArgs e)
        {
            foreach (TMXAssetEditor editor in conditionals)
                if (e.NewLocation is GameLocation gl && gl.mapPath.Value is string mp)
                {
                    if (!getAssetNameWithoutFolder(mp).Equals(getAssetNameWithoutFolder(editor.assetName)))
                        continue;

                    if (PyTK.PyUtils.checkEventConditions(editor.conditions) is bool c && c != editor.lastCheck)
                    {
                        editor.lastCheck = c;
                        Helper.Content.InvalidateCache(e.NewLocation.mapPath.Value);
                        e.NewLocation.updateSeasonalTileSheets();
                    }
                }

            foreach (string map in festivals)
                if(e.NewLocation is GameLocation gl && gl.mapPath.Value is string mp && mp.Contains(map))
                    Helper.Content.InvalidateCache(e.NewLocation.mapPath.Value);
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            if (config.converter)
            {
                exportAllMaps();
                convert();
            }
            loadContentPacks();
            setTileActions();

            PyLua.registerType(typeof(Map), false, true);
            PyLua.registerType(typeof(TMXActions), false, false);
            PyLua.addGlobal("TMX", new TMXActions());
            new ConsoleCommand("loadmap", "Teleport to a map", (s, st) => {
                if (st.Length < 3)
                    monitor.Log("Mising Parameter. Use: loadmap {Map} {x} {y}");
                else
                    Game1.player.warpFarmer(new Warp(int.Parse(st[1]), int.Parse(st[2]), st[0], int.Parse(st[1]), int.Parse(st[2]), false));
            }).register();

            new ConsoleCommand("removeNPC", "Removes an NPC", (s, st) => {
                if (Game1.getCharacterFromName(st.Last()) == null)
                    Monitor.Log("Couldn't find NPC with that name!", LogLevel.Alert);
                else
                {
                    Game1.removeThisCharacterFromAllLocations(Game1.getCharacterFromName(st.Last()));
                    if (Game1.player.friendshipData.ContainsKey(st.Last()))
                        Game1.player.friendshipData.Remove(st.Last());
                    Monitor.Log(st.Last() + " was removed!", LogLevel.Info);
                }

            }).register();

            fixCompatibilities();
            harmonyFix();
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

                syncedFarmers.AddOrReplace(farmer);
            }
        }

        private void fixCompatibilities()
        {
            Compatibility.CustomFarmTypes.LoadingTheMaps();
            helper.Events.GameLoop.SaveLoaded +=(s,e) => Compatibility.CustomFarmTypes.fixGreenhouseWarp();
        }

        private void harmonyFix()
        {
            HarmonyInstance instance = HarmonyInstance.Create("Platonymous.TMXLoader");
            instance.PatchAll(Assembly.GetExecutingAssembly());
        }

        private void setTileActions()
        {
            TileAction Lock = new TileAction("Lock", TMXActions.lockAction).register();
            TileAction Say = new TileAction("Say", TMXActions.sayAction).register();
            TileAction SwitchLayers = new TileAction("SwitchLayers", TMXActions.switchLayersAction).register();
            TileAction Confirm = new TileAction("Confirm", TMXActions.confirmAction).register();
            TileAction OpenShop = new TileAction("OpenShop", TMXActions.shopAction).register();
        }

        private GameLocation addLocation(MapEdit edit)
        {
            GameLocation location;
            Monitor.Log("Adding:" + edit.name);
            if (edit.type == "Deco")
                location = new DecoratableLocation(Path.Combine("Maps", edit.name), edit.name);
            else
                location = new GameLocation(Path.Combine("Maps", edit.name), edit.name);

            if (edit._map.Properties.ContainsKey("Outdoors") && edit._map.Properties["Outdoors"] == "F")
            {
                location.IsOutdoors = false;
                try
                {
                    location.loadLights();
                }
                catch
                {

                }
                location.IsOutdoors = false;
            }

            if (edit._map.Properties.ContainsKey("IsGreenHouse"))
                location.IsGreenhouse = true;

            if (edit._map.Properties.ContainsKey("IsStructure"))
                location.isStructure.Value = true;

            if (edit._map.Properties.ContainsKey("IsFarm"))
                location.IsFarm = true;

            if (!Game1.locations.Contains(location))
                Game1.locations.Add(location);

            location.seasonUpdate(Game1.currentSeason);

            return location;

        }

        private void loadContentPacks()
        {
            foreach (StardewModdingAPI.IContentPack pack in Helper.ContentPacks.GetOwned())
            {
                TMXContentPack tmxPack = pack.ReadJsonFile<TMXContentPack>("content.json");

                if (tmxPack.scripts.Count > 0)
                    foreach (string script in tmxPack.scripts)
                        PyLua.loadScriptFromFile(Path.Combine(pack.DirectoryPath, script), pack.Manifest.UniqueID);

                PyLua.loadScriptFromFile(Path.Combine(Helper.DirectoryPath, "sr.lua"), "Platonymous.TMXLoader.SpouseRoom");

                List<MapEdit> spouseRoomMaps = new List<MapEdit>();
                foreach (SpouseRoom room in tmxPack.spouseRooms)
                {
                    if(room.tilefix && !Overrides.NPCs.Contains(room.name))
                        Overrides.NPCs.Add(room.name);

                    if (room.file != "none")
                    {
                        spouseRoomMaps.Add(new MapEdit() { info = room.name, name = "FarmHouse1_marriage", file = room.file, position = new[] { 29, 1 } });
                        spouseRoomMaps.Add(new MapEdit() { info = room.name, name = "FarmHouse2_marriage", file = room.file, position = new[] { 35, 10 } });
                    }
                }

                foreach (MapEdit edit in spouseRoomMaps)
                {
                    string filePath = Path.Combine(pack.DirectoryPath, edit.file);
                    Map map = TMXContent.Load(edit.file, Helper, pack);

                    Helper.Content.AssetEditors.Add(new TMXAssetEditor(edit, map, EditType.SpouseRoom));
                  //  mapsToSync.AddOrReplace(edit.name, map);
                }

                foreach (TileShop shop in tmxPack.shops)
                {
                    tileShops.AddOrReplace(shop.id, shop.inventory);
                    foreach (string path in shop.portraits)
                        pack.LoadAsset<Texture2D>(path).inject(@"Portraits/"+Path.GetFileNameWithoutExtension(path));
                }

                foreach (NPCPlacement edit in tmxPack.festivalSpots)
                {
                    festivals.AddOrReplace(edit.map);
                    Helper.Content.AssetEditors.Add(new TMXAssetEditor(edit, EditType.Festival));
                   // mapsToSync.AddOrReplace(edit.map, original);
                }

                foreach (NPCPlacement edit in tmxPack.placeNPCs)
                {
                    helper.Events.GameLoop.SaveLoaded += (s, e) =>
                    {
                        if (Game1.getCharacterFromName(edit.name) == null)
                            Game1.locations.Where(gl => gl.Name == edit.map).First().addCharacter(new NPC(new AnimatedSprite("Characters\\" + edit.name, 0, 16, 32), new Vector2(edit.position[0], edit.position[1]), edit.map, 0, edit.name, edit.datable, (Dictionary<int, int[]>)null, Helper.Content.Load<Texture2D>("Portraits\\" + edit.name, ContentSource.GameContent)));
                    };
                }

                foreach (MapEdit edit in tmxPack.addMaps)
                {
                    string filePath = Path.Combine(pack.DirectoryPath, edit.file);
                    Map map = TMXContent.Load(edit.file, Helper,pack);
                    TMXAssetEditor.editWarps(map, edit.addWarps, edit.removeWarps, map);
                    map.inject("Maps/" + edit.name);

                    edit._map = map;
                    if(edit.addLocation)
                        addedLocations.Add(edit);
                    //mapsToSync.AddOrReplace(edit.name, map);
                }

                foreach (MapEdit edit in tmxPack.replaceMaps)
                {
                    string filePath = Path.Combine(pack.DirectoryPath, edit.file);
                    Map map = TMXContent.Load(edit.file, Helper, pack);
                    addAssetEditor(new TMXAssetEditor(edit, map, EditType.Replace));
                   // mapsToSync.AddOrReplace(edit.name, map);
                }

                foreach (MapEdit edit in tmxPack.mergeMaps)
                {
                    string filePath = Path.Combine(pack.DirectoryPath, edit.file);
                    Map map = TMXContent.Load(edit.file, Helper, pack);
                    addAssetEditor(new TMXAssetEditor(edit, map, EditType.Merge));
                   // mapsToSync.AddOrReplace(edit.name, map);
                }

                foreach (MapEdit edit in tmxPack.onlyWarps)
                {
                    addAssetEditor(new TMXAssetEditor(edit, null, EditType.Warps));
                   // mapsToSync.AddOrReplace(edit.name, map);
                }
            }
        }

        private void addAssetEditor(TMXAssetEditor editor)
        {
            if (editor.conditions != "")
                conditionals.Add(editor);

            Helper.Content.AssetEditors.Add(editor);
        }

        private void convert()
        {
            Monitor.Log("Converting..", LogLevel.Trace);
            string inPath = Path.Combine(Helper.DirectoryPath, "Converter", "IN");

            

            string[] directories = Directory.GetDirectories(inPath, "*.*",SearchOption.TopDirectoryOnly);

            foreach (string dir in directories)
            {
                DirectoryInfo inddir = new DirectoryInfo(dir);
                DirectoryInfo outdir = new DirectoryInfo(dir.Replace("IN", "OUT"));
                if (!outdir.Exists)
                    outdir.Create();

                foreach (FileInfo file in inddir.GetFiles())
                {
                    string importPath = Path.Combine("Converter","IN",inddir.Name,file.Name);
                    string exportPath = Path.Combine("Converter", "OUT", inddir.Name, file.Name).Replace(".xnb", ".tmx").Replace(".tbin", ".tmx");
                    if (TMXContent.Convert(importPath, exportPath, Helper, ContentSource.ModFolder, Monitor))
                        file.Delete();
                }
            }
            Monitor.Log("..Done!", LogLevel.Trace);
        }

        private void exportAllMaps()
        {
            string exportFolderPath = Path.Combine(Helper.DirectoryPath, "Converter", "FullMapExport");
            DirectoryInfo exportFolder = new DirectoryInfo(exportFolderPath);
            DirectoryInfo modFolder = new DirectoryInfo(Helper.DirectoryPath);
            string contentPath = PyUtils.ContentPath;

            if (!exportFolder.Exists && contentPath != null && contentPath != "")
                exportFolder.Create();
            else
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
                    map = Helper.Content.Load<Map>(path, ContentSource.GameContent);
                    map.LoadTileSheets(Game1.mapDisplayDevice);
                }
                catch
                {
                    continue;
                }

                if (map == null)
                    continue;

                string exportPath = Path.Combine(exportFolderPath, fileName.Replace(".xnb", ".tmx"));
                TMXContent.Save(map, exportPath, true, Monitor);
            }
        }
       
    }
}
