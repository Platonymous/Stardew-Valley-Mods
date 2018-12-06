using Microsoft.Xna.Framework;
using PyTK;
using PyTK.Extensions;
using PyTK.Lua;
using PyTK.Tiled;
using PyTK.Types;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using System.Collections.Generic;
using System.IO;
using xTile;
using xTile.Layers;
using SFarmer = StardewValley.Farmer;
using Harmony;
using System.Reflection;
using System.Linq;
using xTile.Tiles;
using Microsoft.Xna.Framework.Graphics;

namespace TMXLoader
{
    public class TMXLoaderMod : Mod
    {
        internal static string contentFolder = "Maps";
        internal static IMonitor monitor;
        internal static IModHelper helper;
        internal static Dictionary<string, Map> mapsToSync = new Dictionary<string, Map>();
        internal static List<SFarmer> syncedFarmers = new List<SFarmer>();
        internal static Config config;

        public override void Entry(IModHelper helper)
        {
            config = Helper.ReadConfig<Config>();
            TMXLoaderMod.helper = Helper;
            monitor = Monitor;
            if (config.converter)
            {
                exportAllMaps();
                convert();
            }
            loadContentPacks();
            setTileActions();
            PlayerEvents.Warped += LocationEvents_CurrentLocationChanged;
            TimeEvents.AfterDayStarted += TimeEvents_AfterDayStarted;
            PyLua.registerType(typeof(Map), false, true);
            PyLua.registerType(typeof(TMXActions), false, false);
            PyLua.addGlobal("TMX", new TMXActions());
            new ConsoleCommand("loadmap", "Teleport to a map", (s, st) => {

                Game1.player.warpFarmer(new Warp(int.Parse(st[1]), int.Parse(st[2]), st[0], int.Parse(st[1]), int.Parse(st[2]), false));

            });
            fixCompatibilities();
            harmonyFix();
            
            GameEvents.OneSecondTick += (s, e) =>
            {
                if (Context.IsWorldReady && Game1.IsServer && Game1.IsMultiplayer)
                    if (Game1.otherFarmers.Values.Where(f => f.isActive() && !syncedFarmers.Contains(f)) is IEnumerable<SFarmer> ef && ef.Count() is int i && i > 0)
                        syncMaps(ef);
            };

            TimeEvents.AfterDayStarted += (s, e) => {

                List<SFarmer> toRemove = new List<SFarmer>();

                foreach (SFarmer farmer in syncedFarmers)
                    if (!Game1.otherFarmers.ContainsKey(farmer.UniqueMultiplayerID) || !Game1.otherFarmers[farmer.UniqueMultiplayerID].isActive())
                        toRemove.Add(farmer);

                foreach (SFarmer remove in toRemove)
                    syncedFarmers.Remove(remove);

            };
        }

        private void TimeEvents_AfterDayStarted(object sender, EventArgs e)
        {
            if (Game1.currentLocation is GameLocation g && g.map is Map m && m.Properties.ContainsKey("EntryAction"))
                TileAction.invokeCustomTileActions("EntryAction", g, Vector2.Zero, "Map");
        }

        private void syncMaps(IEnumerable<SFarmer> farmers)
        {
            foreach (SFarmer farmer in farmers)
            {
                foreach (KeyValuePair<string, Map> map in mapsToSync)
                    PyNet.syncMap(map.Value, map.Key, Game1.player);

                syncedFarmers.AddOrReplace(farmer);
            }
        }

        private void fixCompatibilities()
        {
            Compatibility.CustomFarmTypes.LoadingTheMaps();
            SaveEvents.AfterLoad +=(s,e) => Compatibility.CustomFarmTypes.fixGreenhouseWarp();
        }

        private void harmonyFix()
        {
            HarmonyInstance instance = HarmonyInstance.Create("Platonymous.TMXLoader");
            instance.PatchAll(Assembly.GetExecutingAssembly());
        }

        private void LocationEvents_CurrentLocationChanged(object sender, EventArgsPlayerWarped e)
        {
            if (e.NewLocation is GameLocation g && g.map is Map m && m.Properties.ContainsKey("EntryAction"))
                TileAction.invokeCustomTileActions("EntryAction", g, Vector2.Zero,"Map");
        }

        private void setTileActions()
        {
            TileAction Lock = new TileAction("Lock", TMXActions.lockAction).register();
            TileAction Say = new TileAction("Say", TMXActions.sayAction).register();
            TileAction SwitchLayers = new TileAction("SwitchLayers", TMXActions.switchLayersAction).register();
            TileAction Lua = new TileAction("Lua", TMXActions.luaAction).register();
            TileAction Confirm = new TileAction("Confirm", TMXActions.confirmAction).register();
            TileAction Game = new TileAction("Game", TMXActions.gameAction).register();
        }

        private void loadContentPacks()
        {
            foreach (StardewModdingAPI.IContentPack pack in Helper.GetContentPacks())
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

                    if(edit.info != "none")
                        foreach (Layer layer in map.Layers)
                            layer.Id = layer.Id.Replace("Spouse", edit.info);

                    map.Properties.Add("EntryAction", "Lua Platonymous.TMXLoader.SpouseRoom entry");
                    Map original = Helper.Content.Load<Map>("Maps/" + edit.name, ContentSource.GameContent);
                    map = map.mergeInto(original, new Vector2(edit.position[0], edit.position[1]), null, true);
                    map.injectAs("Maps/" + edit.name);
                    mapsToSync.AddOrReplace(edit.name, map);
                }

                foreach (NPCPlacement edit in tmxPack.festivalSpots)
                {
                    Map reference = Helper.Content.Load<Map>("Maps/Town", ContentSource.GameContent);
                    Map original = Helper.Content.Load<Map>("Maps/" + edit.map, ContentSource.GameContent);
                    Texture2D springTex = Helper.Content.Load<Texture2D>("Maps/spring_outdoorsTileSheet", ContentSource.GameContent);
                    Dictionary<string, string> source = Helper.Content.Load<Dictionary<string, string>>("Data\\NPCDispositions",ContentSource.GameContent);
                    int index = source.Keys.ToList().IndexOf(edit.name);
                    TileSheet spring = original.GetTileSheet("ztemp");
                    if (spring == null) {
                        spring = new TileSheet("ztemp", original, "Maps/spring_outdoorsTileSheet", new xTile.Dimensions.Size(springTex.Width,springTex.Height), original.TileSheets[0].TileSize);
                        original.AddTileSheet(spring);
                    }
                    original.GetLayer("Set-Up").Tiles[edit.position[0], edit.position[1]] = new StaticTile(original.GetLayer("Set-Up"), spring, BlendMode.Alpha, (index * 4) + edit.direction);
                    original.injectAs("Maps/" + edit.map);
                    mapsToSync.AddOrReplace(edit.map, original);
                }

                foreach (MapEdit edit in tmxPack.addMaps)
                {
                    string filePath = Path.Combine(pack.DirectoryPath, edit.file);
                    Map map = TMXContent.Load(edit.file, Helper,pack);
                    editWarps(map, edit.addWarps, edit.removeWarps, map);
                    map.inject("Maps/" + edit.name);
                    GameLocation location;
                    if (map.Properties.ContainsKey("Outdoors") && map.Properties["Outdoors"] == "F")
                    {
                        location = new GameLocation(Path.Combine("Maps", edit.name), edit.name) { IsOutdoors = false };
                        location.loadLights();
                        location.IsOutdoors = false;
                    }
                    else
                        location = new GameLocation(Path.Combine("Maps", edit.name), edit.name);

                    location.seasonUpdate(Game1.currentSeason);
                    mapsToSync.AddOrReplace(edit.name, map);
                    SaveEvents.AfterLoad += (s, e) => Game1.locations.Add(location);
                }

                foreach (MapEdit edit in tmxPack.replaceMaps)
                {
                    string filePath = Path.Combine(pack.DirectoryPath, edit.file);
                    Map map = TMXContent.Load(edit.file, Helper, pack);
                    Map original = edit.retainWarps ? Helper.Content.Load<Map>("Maps/" + edit.name, ContentSource.GameContent) : map;
                    editWarps(map, edit.addWarps, edit.removeWarps, original);
                    map.injectAs("Maps/" + edit.name);
                    mapsToSync.AddOrReplace(edit.name, map);
                }

                foreach (MapEdit edit in tmxPack.mergeMaps)
                {
                    string filePath = Path.Combine(pack.DirectoryPath, edit.file);
                    Map map = TMXContent.Load(edit.file, Helper, pack);

                    Map original = Helper.Content.Load<Map>("Maps/" + edit.name, ContentSource.GameContent);
                    Rectangle? sourceArea = null;

                    if (edit.sourceArea.Length == 4)
                        sourceArea = new Rectangle(edit.sourceArea[0], edit.sourceArea[1], edit.sourceArea[2], edit.sourceArea[3]);

                    map = map.mergeInto(original, new Vector2(edit.position[0], edit.position[1]), sourceArea, edit.removeEmpty);
                    editWarps(map, edit.addWarps, edit.removeWarps, original);
                    map.injectAs("Maps/" + edit.name);
                    mapsToSync.AddOrReplace(edit.name, map);
                }

                foreach (MapEdit edit in tmxPack.onlyWarps)
                {
                    Map map = Helper.Content.Load<Map>("Maps/" + edit.name, ContentSource.GameContent);
                    editWarps(map, edit.addWarps, edit.removeWarps, map);
                    map.injectAs("Maps/" + edit.name);
                    mapsToSync.AddOrReplace(edit.name, map);
                }
            }
        }

        private void editWarps(Map map, string[] addWarps, string[] removeWarps, Map original = null)
        {
            if (!map.Properties.ContainsKey("Warp"))
                map.Properties.Add("Warp", "");

            string warps = "";

            if (original != null && original.Properties.ContainsKey("Warp") && !(removeWarps.Length > 0 && removeWarps[0] == "all"))
                warps = original.Properties["Warp"];

            if(addWarps.Length > 0)
                warps = ( warps.Length > 9 ? warps + " " : "") + String.Join(" ", addWarps);

            if(removeWarps.Length > 0 && removeWarps[0] != "all")
            {
                foreach(string warp in removeWarps)
                {
                    warps = warps.Replace(warp + " ", "");
                    warps = warps.Replace(" " + warp, "");
                    warps = warps.Replace(warp, "");
                }
            }

            map.Properties["Warp"] = warps;
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
            string contentPath = PyUtils.getContentFolder();

            if (!exportFolder.Exists && contentPath != null)
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
