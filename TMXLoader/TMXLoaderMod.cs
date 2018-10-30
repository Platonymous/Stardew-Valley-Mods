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

namespace TMXLoader
{
    public class TMXLoaderMod : Mod
    {
        internal static string contentFolder = "Maps";
        internal static IMonitor monitor;
        internal static IModHelper helper;
        internal static Dictionary<string, Map> mapsToSync = new Dictionary<string, Map>();
        internal static List<SFarmer> syncedFarmers = new List<SFarmer>();

        public override void Entry(IModHelper helper)
        {
            TMXLoaderMod.helper = Helper;
            monitor = Monitor;
            Monitor.Log("Environment:" + PyUtils.getContentFolder());
            exportAllMaps();
            convert();
            loadContentPacks();
            setTileActions();
            PlayerEvents.Warped += LocationEvents_CurrentLocationChanged;
            TimeEvents.AfterDayStarted += TimeEvents_AfterDayStarted;
            PyLua.registerType(typeof(Map), false, true);
            PyLua.registerType(typeof(TMXActions), false, false);
            PyLua.addGlobal("TMX", new TMXActions());
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
        }

        private void loadContentPacks()
        {
            foreach (StardewModdingAPI.IContentPack pack in Helper.GetContentPacks())
            {
                TMXContentPack tmxPack = pack.ReadJsonFile<TMXContentPack>("content.json");

                if (tmxPack.scripts.Count > 0)
                    foreach (string script in tmxPack.scripts)
                        PyLua.loadScriptFromFile(Path.Combine(pack.DirectoryPath, script), pack.Manifest.UniqueID);

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
