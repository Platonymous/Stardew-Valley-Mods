using Microsoft.Xna.Framework;
using PyTK;
using PyTK.Extensions;
using PyTK.Tiled;
using PyTK.Types;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using xTile;
using xTile.Layers;

namespace TMXLoader
{
    public class TMXLoaderMod : Mod
    {
        internal static string contentFolder = "Maps";

        public override void Entry(IModHelper helper)
        {
            exportAllMaps();
            convert();
            loadContentPacks();
            setTileActions();
        }

        private void setTileActions()
        {
            TileAction Lock = new TileAction("Lock", lockAction).register();
            TileAction Say = new TileAction("Say", sayAction).register();
            TileAction SwitchLayers = new TileAction("SwitchLayers", switchLayersAction).register();
        }

        private bool sayAction(string action, GameLocation location, Vector2 tile, string layer)
        {
            List<string> text = action.Split(' ').ToList();
            bool inDwarvish = false;
            
            if (text[1] == ("Dwarvish"))
            {
                text.RemoveAt(1);
                if (!Game1.player.canUnderstandDwarves)
                    inDwarvish = true;
            }
            text.RemoveAt(0);
            action = String.Join(" ", text);
            action = inDwarvish ? Dialogue.convertToDwarvish(action) : action;

            Game1.drawDialogueNoTyping(action); return true;
        }

        private bool switchLayersAction(string action, GameLocation location, Vector2 tile, string layer)
        {
            string[] actions = action.Split(' ');

            foreach (string s in actions)
            {
                string[] layers = s.Split(':');
                if (layers.Length > 1)
                {
                    if(layers.Length < 4)
                        location.map.switchLayers(layers[0], layers[1]);
                    else
                    {
                        string[] xStrings = layers[2].Split('-');
                        string[] yStrings = layers[3].Split('-');
                        Range xRange = new Range(int.Parse(xStrings[0]), int.Parse(xStrings.Last()) + 1);
                        Range yRange = new Range(int.Parse(yStrings[0]), int.Parse(yStrings.Last()) + 1);

                        foreach(int x in xRange.toArray())
                            foreach(int y in yRange.toArray())
                                location.map.switchTileBetweenLayers(layers[0], layers[1], x, y);
                    }
                }
                    
            }

            return true;
        }

        private bool lockAction(string action, GameLocation location, Vector2 tile, string layer)
        {
            string[] strings = action.Split(' ');

            if (Game1.player.ActiveObject is Item i && i.parentSheetIndex == int.Parse(strings[2]) && i.Stack >= int.Parse(strings[1]))
            {
                int amount = int.Parse(strings[1]);
                Game1.playSound("newArtifact");

                if (i.Stack > amount)
                    i.Stack -= amount;
                else
                    Game1.player.removeItemFromInventory(i);

                TileAction.invokeCustomTileActions("Success", location, tile, layer);
            }
            else if (Game1.player.ActiveObject == null)
                TileAction.invokeCustomTileActions("Default", location, tile, layer);
            else
                TileAction.invokeCustomTileActions("Failure", location, tile, layer);

            return true;
        }

        private void loadContentPacks()
        {
            List<TMXContentPack> packs = new List<TMXContentPack>();
            PyUtils.loadContentPacks<TMXContentPack>(out packs, Path.Combine(Helper.DirectoryPath, contentFolder), SearchOption.AllDirectories, Monitor);

            foreach (TMXContentPack pack in packs)
            {
                foreach (MapEdit edit in pack.addMaps)
                {
                    string filePath = Path.Combine(contentFolder, pack.folderName, edit.file);
                    Map map = TMXContent.Load(filePath, Helper);
                    editWarps(map, edit.addWarps, edit.removeWarps, map);
                    Monitor.Log(":" + map.Properties["Warp"] + ":");
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

                    map = map.mergeInto(original, new Vector2(edit.position[0], edit.position[1]), sourceArea);
                    editWarps(map, edit.addWarps, edit.removeWarps, original);
                    addMoreMapLayers(map);
                    map.injectAs("Maps/" + edit.name);
                }

                foreach (MapEdit edit in pack.onlyWarps)
                {
                    Map map = Helper.Content.Load<Map>("Maps/" + edit.name, ContentSource.GameContent);
                    editWarps(map, edit.addWarps, edit.removeWarps, map);
                    Monitor.Log(":" + map.Properties["Warp"] + ":");
                    map.injectAs("Maps/" + edit.name);
                }
            }
        }

        private void addMoreMapLayers(Map map)
        {
            foreach(Layer layer in map.Layers)
                if (layer.Properties.ContainsKey("Draw") && map.GetLayer(layer.Properties["Draw"]) is Layer maplayer)
                    maplayer.AfterDraw += (s,e) => layer.Draw(Game1.mapDisplayDevice, Game1.viewport, xTile.Dimensions.Location.Origin, false, Game1.pixelZoom);
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
            string[] files = Directory.GetFiles(inPath, "*.*", SearchOption.TopDirectoryOnly);
            foreach (string file in files)
            {
                string fileName = new FileInfo(file).Name;
                string importPath = Path.Combine("Converter", "IN", fileName);
                FileInfo importFile = new FileInfo(Path.Combine(inPath, fileName));
                string exportPath = Path.Combine("Converter", "OUT", fileName.Replace(".xnb", ".tmx").Replace(".tbin", ".tmx"));
                if (TMXContent.Convert(importPath, exportPath, Helper, ContentSource.ModFolder, Monitor))
                    importFile.Delete();
            }
            Monitor.Log("..Done!", LogLevel.Trace);
        }

        private void exportAllMaps()
        {
            string exportFolderPath = Path.Combine(Helper.DirectoryPath, "Converter", "FullMapExport");
            DirectoryInfo exportFolder = new DirectoryInfo(exportFolderPath);
            DirectoryInfo modFolder = new DirectoryInfo(Helper.DirectoryPath);
            string contentPath = Path.Combine(modFolder.Parent.Parent.FullName, "Content");

            if (!exportFolder.Exists)
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
