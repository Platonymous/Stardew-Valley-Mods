using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using PyTK;
using PyTK.Extensions;
using PyTK.Tiled;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xTile;
using xTile.ObjectModel;

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
                    GameLocation location = new GameLocation(map, edit.name);
                    if (map.Properties.ContainsKey("Outdoors") && map.Properties["Outdoors"] == "F")
                    {
                        location.isOutdoors = false;
                        location.loadLights();
                    }
                        

                    SaveEvents.AfterLoad += (s, e) => Game1.locations.Add(new GameLocation(map, edit.name) { isOutdoors = false });
                }

                foreach (MapEdit edit in pack.replaceMaps)
                {
                    string filePath = Path.Combine(contentFolder, pack.folderName, edit.file);
                    Map map = TMXContent.Load(filePath, Helper);
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
