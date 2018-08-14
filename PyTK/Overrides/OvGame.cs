using Harmony;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley.Menus;
using System.Reflection;

namespace PyTK.Overrides
{
    internal class OvReadyCheck
    {
        internal static IModHelper Helper { get; } = PyTKMod._helper;
        internal static IMonitor Monitor { get; } = PyTKMod._monitor;
        internal static bool allready = false;

        [HarmonyPatch]
        internal class AllReaday
        {
            internal static MethodInfo TargetMethod()
            {
                return AccessTools.Method(PyUtils.getTypeSDV("Menus.ReadyCheckDialog"), "update");
            }

            internal static void Postfix(ReadyCheckDialog __instance, GameTime time)
            {
                if(allready)
                __instance.confirm();
                allready = false;
            }
        }

    }
}
