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

namespace TMXLoader
{
    public static class TMXContent
    {
        internal static IModHelper Helper = TMXLoaderMod.helper;
        internal static IMonitor Monitor = TMXLoaderMod.monitor;
        internal static List<string> Injected = new List<string>();


        public static Map Load(string path, IModHelper helper, IContentPack contentPack = null)
        {
            string content = "Content";
            if (contentPack is IContentPack cp)
                content = cp.Manifest.UniqueID;
            Monitor.Log("Loading Map: " + content + ">" + path, LogLevel.Trace);
                return Load(path, helper, false, contentPack);
            
        }

        public static Map Load(string path, IModHelper helper, bool syncTexturesToClients, IContentPack contentPack)
        {
            Map map = contentPack != null ? contentPack.ModContent.Load<Map>(path) : helper.ModContent.Load<Map>(path);

            for (int index = 0; index < map.TileSheets.Count; ++index)
                if (string.IsNullOrWhiteSpace(Path.GetDirectoryName(map.TileSheets[index].ImageSource)))
                    map.TileSheets[index].ImageSource = Path.Combine("Maps", Path.GetFileName(map.TileSheets[index].ImageSource));

                map?.LoadTileSheets(Game1.mapDisplayDevice);
       
            return map;
        }

        public static void Save(Map map, string path, bool includeTilesheets = false, IMonitor monitor = null)
        {
            FileInfo pathFile = new FileInfo(path);
            //Dictionary<TileSheet, Texture2D> tilesheets = Helper.Reflection.GetField<Dictionary<TileSheet, Texture2D>>(Game1.mapDisplayDevice, "m_tileSheetTextures").GetValue();
            if (pathFile.Exists)
                return;
            
            if (includeTilesheets && PyDisplayDevice.Instance is PyDisplayDevice pdd)
                foreach (TileSheet ts in map.TileSheets)
                {
                    string folder = Path.Combine(Path.GetDirectoryName(path), Path.GetDirectoryName(ts.ImageSource));

                    if (!Directory.Exists(folder))
                        Directory.CreateDirectory(folder);

                    var tsTexture = pdd.GetTexture(ts);

                    if (tsTexture == null)
                        continue;

                    string exportPath = Path.Combine(pathFile.Directory.FullName, Path.GetDirectoryName(ts.ImageSource), Path.GetFileNameWithoutExtension(ts.ImageSource) + ".png");

                    FileInfo exportFile = new FileInfo(exportPath);

                    if (exportFile.Exists)
                        continue;

                    try
                    {
                        tsTexture.SaveAsPng(new FileStream(exportPath, FileMode.Create), tsTexture.Width, tsTexture.Height);

                        if(monitor != null)
                            monitor.Log("Saving: " + exportFile.Name);
                    }
                    catch (Exception e)
                    {
                        Monitor.Log(e.Message, LogLevel.Error);
                        Monitor.Log(e.StackTrace, LogLevel.Error);
/*
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
                        catch(Exception e2)
                        {
                            Monitor.Log(e2.Message, LogLevel.Error);
                            Monitor.Log(e2.StackTrace, LogLevel.Error);
                        }
                        */
                    }
                }

            List<xTile.Layers.Layer> layers = new List<xTile.Layers.Layer>();

            foreach (var layer in map.Layers)
                if (layer.Properties.ContainsKey("DrawChecked"))
                {
                    layers.Add(layer);
                    layer.Properties.Remove("DrawChecked");
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

            foreach (var layer in layers)
                layer.Properties.Add("DrawChecked", true);
        }

        public static bool Convert (string pathIn, string pathOut, IModHelper helper, bool gameContent = false, IMonitor monitor = null)
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
                    mem = tbin2tmx(pathIn, helper, gameContent);
                else
                    mem = xnb2tmx(pathIn, helper, gameContent);

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
                    Texture2D image = helper.ModContent.Load<Texture2D>(pathIn);
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
            path = path.Replace(@"/[TMX Loader]", "");
            Map newMap = FormatManager.Instance.LoadMap(path);
            return newMap;
        }

        internal static MemoryStream map2tmx(Map map)
        {
            MemoryStream stream = new MemoryStream();
            xTile.Format.FormatManager.Instance.GetMapFormatByExtension("tmx").Store(map, stream);
            return stream;
        }

        internal static MemoryStream map2tbin(Map map)
        {
            MemoryStream stream = new MemoryStream();
            FormatManager.Instance.BinaryFormat.Store(map, stream);
            return stream;
        }

        internal static MemoryStream tbin2tmx(string path, IModHelper helper, bool gameContent)
        {
            Map map = gameContent
                ? helper.GameContent.Load<Map>(path)
                : helper.ModContent.Load<Map>(path);
            return map2tmx(map);
        }

        internal static MemoryStream xnb2tmx(string path, IModHelper helper, bool gameContent)
        {
            Map map = gameContent
                ? helper.GameContent.Load<Map>(path)
                : helper.ModContent.Load<Map>(path);
            return map2tmx(map);
        }

        internal static MemoryStream xnb2tbin(string path, IModHelper helper, bool gameContent)
        {
            Map map = gameContent
                ? helper.GameContent.Load<Map>(path)
                : helper.ModContent.Load<Map>(path);
            map.LoadTileSheets(Game1.mapDisplayDevice);
            return map2tbin(map);
        }
    }
}
