using Harmony;
using Microsoft.Xna.Framework;
using PyTK.Types;
using StardewModdingAPI;
using System;
using System.Reflection;

namespace PyTK.Overrides
{
    internal class OvGame
    {
        internal static IModHelper Helper { get; } = PyTKMod._helper;
        internal static IMonitor Monitor { get; } = PyTKMod._monitor;
        internal static bool skipping = false;

        [HarmonyPatch]
        internal class TimeSkipFix
        {
            internal static MethodInfo TargetMethod()
            {
                return AccessTools.Method(Type.GetType("StardewModdingAPI.Framework.SGame, StardewModdingAPI"), "Update");
            }

            internal static bool Prefix(GameTime gameTime)
            {
                return !skipping || (skipping && (gameTime is AltGameTime));
            }
        }

    }
}
