using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System.Collections.Generic;
using System.IO;
using xTile;
using xTile.Format;
using xTile.Tiles;
using System.Linq;
using System;
using PyTK.Extensions;

namespace PyTK.Tiled
{
    public static class TMXContent
    {
        public static IMapFormat TMXFormat = new NewTiledTmxFormat();
        internal static IModHelper Helper = PyTKMod._helper;
        internal static IMonitor Monitor = PyTKMod._monitor;


        public static Map Load(string path, IModHelper helper, IContentPack contentPack = null)
        {
            return Load(path, helper, false, contentPack);
        }

        public static Map LoadAndSyncToClients(string path, IModHelper helper, IContentPack contentPack = null, string assetNameMap = null)
        {
            Map map = Load(path, helper, true, contentPack);

            if (assetNameMap != null)
                if (Game1.IsMultiplayer && Game1.IsServer)
                    foreach (Farmer farmhand in Game1.otherFarmers.Values)
                        PyNet.sendGameContent(assetNameMap, map, farmhand, (b) => Monitor.Log("Syncing " + assetNameMap + " to " + farmhand.Name + ": " + (b ? "successful" : "failed"), b ? LogLevel.Info : LogLevel.Warn));

            return map;
        }

        public static Map Load(string path, IModHelper helper, bool syncTexturesToClients, IContentPack contentPack)
        {
            Dictionary<TileSheet, Texture2D> tilesheets = Helper.Reflection.GetField<Dictionary<TileSheet, Texture2D>>(Game1.mapDisplayDevice, "m_tileSheetTextures").GetValue();
            Map map = tmx2map(Path.Combine(contentPack != null ? contentPack.DirectoryPath : helper.DirectoryPath,path));
            string fileName = new FileInfo(path).Name;

            foreach (TileSheet t in map.TileSheets)
            {
                string[] seasons = new string[] { "summer_", "fall_", "winter_" };
                string tileSheetPath = path.Replace(fileName, t.ImageSource + ".png");

                FileInfo tileSheetFile = new FileInfo(Path.Combine(contentPack != null ? contentPack.DirectoryPath : helper.DirectoryPath, tileSheetPath));
                FileInfo tileSheetFileVanilla = new FileInfo(Path.Combine(PyUtils.getContentFolder(), "Content", t.ImageSource + ".xnb"));
                if (tileSheetFile.Exists && !tileSheetFileVanilla.Exists && tilesheets.Find(k => k.Key.ImageSource == t.ImageSource).Key == null)
                {
                    Texture2D tilesheet = contentPack != null ? contentPack.LoadAsset<Texture2D>(tileSheetPath) : helper.Content.Load<Texture2D>(tileSheetPath);
                    tilesheet.inject(t.ImageSource);

                    if (syncTexturesToClients && Game1.IsMultiplayer && Game1.IsServer)
                        foreach (Farmer farmhand in Game1.otherFarmers.Values)
                            PyNet.sendGameContent(t.ImageSource, tilesheet, farmhand, (b) => Monitor.Log("Syncing " + t.ImageSource + " to " + farmhand.Name + ": " + (b ? "successful" : "failed"), b ? LogLevel.Info : LogLevel.Warn));

                    if (t.ImageSource.Contains("spring_"))
                        foreach (string season in seasons)
                        {
                            string seasonPath = path.Replace(fileName, t.ImageSource.Replace("spring_", season));
                            FileInfo seasonFile = new FileInfo(Path.Combine(contentPack != null ? contentPack.DirectoryPath : helper.DirectoryPath, seasonPath + ".png"));
                            if (seasonFile.Exists && tilesheets.Find(k => k.Key.ImageSource == t.ImageSource.Replace("spring_", season)).Key == null)
                            {
                                Texture2D seasonTilesheet = contentPack != null ? contentPack.LoadAsset<Texture2D>(seasonPath + ".png") : helper.Content.Load<Texture2D>(seasonPath + ".png");
                                string seasonTextureName = t.ImageSource.Replace("spring_", season);
                                seasonTilesheet.inject(seasonTextureName);
                                seasonTilesheet.inject("Maps/" + seasonTextureName);

                                if (syncTexturesToClients && Game1.IsMultiplayer && Game1.IsServer)
                                    foreach (Farmer farmhand in Game1.otherFarmers.Values)
                                        PyNet.sendGameContent(new string[] { seasonTextureName, "Maps/" + seasonTextureName }, seasonTilesheet, farmhand, (b) => Monitor.Log("Syncing " + seasonTextureName + " to " + farmhand.Name + ": " + (b ? "successful" : "failed"), b ? LogLevel.Info : LogLevel.Warn));
                            }
                        }
                }
            }
                map.LoadTileSheets(Game1.mapDisplayDevice);
            return map;
        }

        public static void Save(Map map, string path, bool includeTilesheets = false, IMonitor monitor = null)
        {
            FileInfo pathFile = new FileInfo(path);
            Dictionary<TileSheet, Texture2D> tilesheets = Helper.Reflection.GetField<Dictionary<TileSheet, Texture2D>>(Game1.mapDisplayDevice, "m_tileSheetTextures").GetValue();

            if (pathFile.Exists)
                return;
            
            if (includeTilesheets)
                foreach (TileSheet ts in map.TileSheets)
                {
                    if (ts.ImageSource.Contains('\\'))
                    {
                        string subDirectoryPath = Path.Combine(pathFile.Directory.FullName, ts.ImageSource.Split('\\')[0]);
                        DirectoryInfo subDirectory = new DirectoryInfo(subDirectoryPath);
                        if (!subDirectory.Exists)
                            subDirectory.Create();
                    }

                    if (!tilesheets.ContainsKey(ts))
                        continue;

                    string exportPath = Path.Combine(pathFile.Directory.FullName, ts.ImageSource + ".png");
                    FileInfo exportFile = new FileInfo(exportPath);

                    if (exportFile.Exists)
                        continue;

                    try
                    {
                        tilesheets[ts].SaveAsPng(new FileStream(exportPath, FileMode.Create), tilesheets[ts].Width, tilesheets[ts].Height);

                        if(monitor != null)
                            monitor.Log("Saving: " + exportFile.Name);
                    }
                    catch
                    {
                        try
                        {
                            string normalized = ts.ImageSource.Split('.')[0];
                            string normalizedPath = Path.Combine(pathFile.Directory.FullName, normalized + ".png");
                            FileInfo normalizedFile = new FileInfo(normalizedPath);
                            TileSheet normalizedTileSheet = tilesheets.Keys.ToList().Find(t => t.ImageSource == normalized);

                            if (!normalizedFile.Exists && normalizedTileSheet != null)
                            {
                                tilesheets[normalizedTileSheet].SaveAsPng(new FileStream(normalizedPath, FileMode.Create), tilesheets[normalizedTileSheet].Width, tilesheets[normalizedTileSheet].Height);

                                if (monitor != null)
                                    monitor.Log("Saving: " + normalizedFile.Name);

                                ts.ImageSource = normalized;
                            }
                        }
                        catch
                        {

                        }
                    }
                }

            try
            {
                MemoryStream mem = map2tmx(map);
                FileStream file = new FileStream(path, FileMode.Create);
                if (monitor != null)
                    monitor.Log("Saving: " + pathFile.Name);
                mem.Position = 0;
                mem.CopyTo(file);
            }
            catch
            {

            }
        }

        public static bool Convert (string pathIn, string pathOut, IModHelper helper, ContentSource contentSource = ContentSource.ModFolder, IMonitor monitor = null)
        {
            if(!pathIn.EndsWith(".tbin") && !pathIn.EndsWith(".xnb") && !pathOut.EndsWith(".tmx"))
            {
                new FileInfo(Path.Combine(helper.DirectoryPath, pathIn)).CopyTo(Path.Combine(helper.DirectoryPath, pathOut),true);
                return true;
            }

                if (!pathOut.EndsWith(".tmx"))
                pathOut = pathOut + ".tmx";


            MemoryStream mem = null;
            try
            {
                if (pathIn.EndsWith(".tbin"))
                    mem = tbin2tmx(pathIn, helper, contentSource);
                else
                    mem = xnb2tmx(pathIn, helper, contentSource);

            FileStream file = new FileStream(Path.Combine(helper.DirectoryPath, pathOut), FileMode.Create);
            mem.Position = 0;
            mem.CopyTo(file);

            if (monitor != null)
                monitor.Log($"Converting {pathIn} -> {pathOut}", LogLevel.Trace);
                return true;
            }
            catch (Exception ex)
            {
                Monitor.Log("Map Error" + ex.Message + ":" + ex.StackTrace);

                try
                {
                    string imagePath = pathOut.Replace(".tmx", ".png");
                    Texture2D image = helper.Content.Load<Texture2D>(pathIn);
                    image.SaveAsPng(new FileStream(Path.Combine(helper.DirectoryPath, imagePath), FileMode.Create), image.Width, image.Height);
                    return true;
                }
                catch(Exception e)
                {
                    Monitor.Log("Image Error" + e.Message + ":" + e.StackTrace);
                    return false;
                }
            }
        }

        internal static MemoryStream tmx2tbin(string path)
        {
            Map newMap = FormatManager.Instance.LoadMap(path);

            foreach (TileSheet t in newMap.TileSheets)
                t.ImageSource = t.ImageSource.Contains(".png") ? t.ImageSource : t.ImageSource + ".png";

            MemoryStream stream = new MemoryStream();
            FormatManager.Instance.BinaryFormat.Store(newMap, stream);
            return stream;
        }

        internal static Map tmx2map(string path)
        {
            Map newMap = FormatManager.Instance.LoadMap(path);

            foreach (TileSheet t in newMap.TileSheets)
                t.ImageSource = t.ImageSource.Contains(".png") ? t.ImageSource.Replace(".png","") : t.ImageSource;


            return newMap;
        }

        internal static MemoryStream map2tmx(Map map)
        {
            MemoryStream stream = new MemoryStream();
            TMXFormat.Store(map, stream);
            return stream;
        }

        internal static MemoryStream map2tbin(Map map)
        {
            MemoryStream stream = new MemoryStream();
            FormatManager.Instance.BinaryFormat.Store(map, stream);
            return stream;
        }

        internal static MemoryStream tbin2tmx(string path, IModHelper helper, ContentSource contentSource)
        {
            Map map = helper.Content.Load<Map>(path, contentSource);
            return map2tmx(map);
        }

        internal static MemoryStream xnb2tmx(string path, IModHelper helper, ContentSource contentSource)
        {
            Map map = helper.Content.Load<Map>(path, contentSource);
            return map2tmx(map);
        }

        internal static MemoryStream xnb2tbin(string path, IModHelper helper, ContentSource contentSource)
        {
            Map map = helper.Content.Load<Map>(path, contentSource);
            map.LoadTileSheets(Game1.mapDisplayDevice);
            return map2tbin(map);
        }
    }
}
