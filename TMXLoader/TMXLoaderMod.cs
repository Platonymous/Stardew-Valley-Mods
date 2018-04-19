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

namespace TMXLoader
{
    public class TMXLoaderMod : Mod
    {
        internal static string contentFolder = "Maps";
        internal static IMonitor monitor;
        internal static IModHelper helper;

        public override void Entry(IModHelper helper)
        {
            TMXLoaderMod.helper = Helper;
            monitor = Monitor;
            Monitor.Log("Environment:" + PyUtils.getContentFolder());
            exportAllMaps();
            convert();
            loadContentPacks();
            setTileActions();
            LocationEvents.CurrentLocationChanged += LocationEvents_CurrentLocationChanged;
            PyLua.registerType(typeof(Map), false, true);
            PyLua.registerType(typeof(TMXActions), false, false);
            PyLua.addGlobal("TMX", new TMXActions());
            fixCompatibilities();
            harmonyFix();
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

        private void LocationEvents_CurrentLocationChanged(object sender, EventArgsCurrentLocationChanged e)
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
            List<TMXContentPack> packs = new List<TMXContentPack>();
            PyUtils.loadContentPacks<TMXContentPack>(out packs, Path.Combine(Helper.DirectoryPath, contentFolder), SearchOption.AllDirectories, Monitor);

            foreach (TMXContentPack pack in packs)
            {
                if (pack.scripts.Count > 0)
                    foreach (string script in pack.scripts)
                        PyLua.loadScriptFromFile(Path.Combine(Helper.DirectoryPath, contentFolder, pack.folderName, script), pack.folderName);


                foreach (MapEdit edit in pack.addMaps)
                {
                    string filePath = Path.Combine(contentFolder, pack.folderName, edit.file);
                    Map map = TMXContent.Load(filePath, Helper);
                    editWarps(map, edit.addWarps, edit.removeWarps, map);
                    map.inject("Maps/" + edit.name);
                    addMoreMapLayers(map);
                    GameLocation location;
                    if (map.Properties.ContainsKey("Outdoors") && map.Properties["Outdoors"] == "F")
                    {
                        location = new GameLocation(map, edit.name) { isOutdoors = false };
                        location.loadLights();
                        location.isOutdoors = false;
                    }
                    else
                        location = new GameLocation(map, edit.name);

                    location.seasonUpdate(Game1.currentSeason);

                    SaveEvents.AfterLoad += (s, e) => Game1.locations.Add(location);
                }

                foreach (MapEdit edit in pack.replaceMaps)
                {
                    string filePath = Path.Combine(contentFolder, pack.folderName, edit.file);
                    Map map = TMXContent.Load(filePath, Helper);
                    addMoreMapLayers(map);
                    Map original = edit.retainWarps ? Helper.Content.Load<Map>("Maps/" + edit.name, ContentSource.GameContent) : map;
                    editWarps(map, edit.addWarps, edit.removeWarps, original);
                    map.injectAs("Maps/" + edit.name);
                }

                foreach (MapEdit edit in pack.mergeMaps)
                {
                    string filePath = Path.Combine(contentFolder, pack.folderName, edit.file);
                    Map map = TMXContent.Load(filePath, Helper);

                    Map original = Helper.Content.Load<Map>("Maps/" + edit.name, ContentSource.GameContent);
                    Rectangle? sourceArea = null;

                    if (edit.sourceArea.Length == 4)
                        sourceArea = new Rectangle(edit.sourceArea[0], edit.sourceArea[1], edit.sourceArea[2], edit.sourceArea[3]);

                    map = map.mergeInto(original, new Vector2(edit.position[0], edit.position[1]), sourceArea, true);
                    editWarps(map, edit.addWarps, edit.removeWarps, original);
                    addMoreMapLayers(map);
                    map.injectAs("Maps/" + edit.name);
                }

                foreach (MapEdit edit in pack.onlyWarps)
                {
                    Map map = Helper.Content.Load<Map>("Maps/" + edit.name, ContentSource.GameContent);
                    editWarps(map, edit.addWarps, edit.removeWarps, map);
                    map.injectAs("Maps/" + edit.name);
                }
            }
        }

        

        private void addMoreMapLayers(Map map)
        {
            foreach (Layer layer in map.Layers)
                if (layer.Properties.ContainsKey("Draw") && map.GetLayer(layer.Properties["Draw"]) is Layer maplayer)
                        maplayer.AfterDraw += (s, e) => layer.Draw(Game1.mapDisplayDevice, Game1.viewport, xTile.Dimensions.Location.Origin, false, Game1.pixelZoom);
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
