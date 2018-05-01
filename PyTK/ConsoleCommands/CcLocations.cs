using PyTK.Types;
using StardewValley;
using System;
using StardewModdingAPI;

namespace PyTK.ConsoleCommands
{
    public static class CcLocations
    {
        public static ConsoleCommand clearSpace()
        {
            Action action = delegate ()
             {
                 if (Game1.currentLocation is GameLocation location)
                 {
                     int o = location.objects.Count() + location.largeTerrainFeatures.Count + location.terrainFeatures.Count();

                     location.objects.Clear();
                     location.largeTerrainFeatures.Clear();
                     location.terrainFeatures.Clear();

                     if (location is Farm farm)
                     {
                         o += farm.resourceClumps.Count;
                         farm.resourceClumps.Clear();
                     }

                     PyTKMod._monitor.Log($"Removed {o} objects.", LogLevel.Trace);
                 }
             };

            return new ConsoleCommand("pytk_clearspace", "Removes all Objects and TerrainFeatures from the current Location", (s, p) => action.Invoke());
        }
    }
}
