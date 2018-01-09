using PyTK.Extensions;
using PyTK.Types;
using StardewValley;
using StardewModdingAPI;
using System;
using StardewModdingAPI.Events;
using System.Collections.Generic;
using StardewValley.Locations;
using StardewValley.Buildings;
using SObject = StardewValley.Object;
using System.Linq;
using Microsoft.Xna.Framework;
using System.Reflection;
using System.IO;

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
            List<GameLocation> list = Game1.locations;

            for (int i = 0; i < Game1.locations.Count; i++)
                if (Game1.locations[i] is BuildableGameLocation bgl)
                    for (int j = 0; j < bgl.buildings.Count; j++)
                        if (bgl.buildings[j].indoors != null)
                            list.Add(bgl.buildings[j].indoors);

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
                        string author = p.author == "none" ? "" : " by " + p.author;
                        monitor.Log(p.name + " " + p.version + author, LogLevel.Info);
                    }
                }
            }
        }

    }
}
