using PyTK.Types;
using StardewValley;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using StardewValley.Locations;
using System.IO;
using System.Linq;
using StardewValley.Buildings;
using System.Xml.Schema;
using System.Xml;

namespace PyTK
{
    public static class PyUtils
    {
        internal static IModHelper Helper { get; } = PyTKMod._helper;
        internal static IMonitor Monitor { get; } = PyTKMod._monitor;

        public static bool CheckEventConditions(string conditions)
        {
            return Helper.Reflection.GetMethod(Game1.currentLocation, "checkEventPrecondition").Invoke<int>(new object[] { "9999999/" + conditions }) != -1;
        }

        public static List<GameLocation> getAllLocationsAndBuidlings()
        {
            List<GameLocation> list = Game1.locations.ToList();

            foreach (GameLocation location in Game1.locations)
                if (location is BuildableGameLocation bgl)
                    foreach (Building building in bgl.buildings)
                        if (building.indoors != null)
                            list.Add(building.indoors);

            return list;
        }

        public static DelayedAction setDelayedAction(int delay, Action action)
        {
            DelayedAction d = new DelayedAction(delay, () => action());
            Game1.delayedActions.Add(d);
            return d;
        }

        public static void loadContentPacks<TModel>(out List<TModel> packs, string folder, SearchOption option = SearchOption.AllDirectories, IMonitor monitor = null, string filesearch = "*.json") where TModel : class
        {
            packs = new List<TModel>();
            string[] files = Directory.GetFiles(folder, filesearch, option);
            foreach (string file in files)
            {
                TModel pack = Helper.ReadJsonFile<TModel>(file);
                packs.Add(pack);

                if (pack is IContentPack p)
                {
                    p.fileName = new FileInfo(file).Name;
                    p.folderName = new FileInfo(file).Directory.Name;

                    if (monitor != null)
                    {
                        string author = p.author == "none" || p.author == null || p.author == "" ? "" : " by " + p.author;
                        monitor.Log(p.name + " " + p.version + author, LogLevel.Info);
                    }
                }
            }
        }

        public static Type getTypeSDV(string type)
        {
            string prefix = "StardewValley.";
            Type defaulSDV = Type.GetType(prefix + type + ", Stardew Valley");

            if (defaulSDV != null)
                return defaulSDV;
            else
                return Type.GetType(prefix + type + ", StardewValley");

        }

        internal static void checkAllSaves()
        {
            string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StardewValley", "Saves");
            string[] files = Directory.GetFiles(folder, "*", SearchOption.AllDirectories);
            foreach (string file in files)
            {
                if(!file.Contains(".") && !file.Contains("old") && !file.Contains("SaveGame"))
                {
                        XmlReaderSettings settings = new XmlReaderSettings();

                        XmlReader reader = XmlReader.Create(Path.Combine(Helper.DirectoryPath, file), settings);
                    FileInfo info = new FileInfo(file);
                    try
                    {
                        while (reader.Read());
                        Monitor.Log( info.Directory.Name + "/" + info.Name + " (OK)");
                    }
                    catch (Exception e)
                    {
                        Monitor.Log("Error in " + info.Directory.Name + "/" + info.Name + ": " + e.Message,LogLevel.Error);
                    }
                }
            }           
        }
    }
}
