using PyTK.Extensions;
using StardewValley;
using StardewModdingAPI;
using System;
using StardewModdingAPI.Events;
using System.Collections.Generic;
using StardewValley.Locations;
using StardewValley.Buildings;

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

            foreach (GameLocation gl in Game1.locations)
                if (gl is BuildableGameLocation bgl)
                    foreach (Building b in bgl.buildings)
                        list.Add(b.indoors);

            return list;
        }
    }
}
