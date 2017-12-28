using PyTK.Extensions;
using StardewValley;
using StardewModdingAPI;
using System;
using StardewModdingAPI.Events;

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
    }
}
