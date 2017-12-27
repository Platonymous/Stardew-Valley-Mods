using PyTK.Extensions;
using StardewValley;
using StardewModdingAPI;

namespace PyTK
{
    public static class PyUtils
    {
        internal static IModHelper Helper { get; } = PyTKMod._helper;
        internal static IMonitor Monitor { get; } = PyTKMod._monitor;


    }
}
