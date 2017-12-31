using Harmony;
using StardewValley.Objects;
using System;
using System.Reflection;
using SFarmer = StardewValley.Farmer;
using PyTK.CustomTV;

namespace PyTK.Overrides
{
    internal class OvTV
    {
        [HarmonyPatch]
        internal class TVFix
        {
            internal static MethodInfo TargetMethod()
            {
                if (Type.GetType("StardewValley.Objects.TV, Stardew Valley") != null)
                    return AccessTools.Method(Type.GetType("StardewValley.Objects.TV, Stardew Valley"), "checkForAction");
                else
                    return AccessTools.Method(Type.GetType("StardewValley.Objects.TV, StardewValley"), "checkForAction");
            }

            internal static bool Prefix(ref TV __instance, SFarmer who, bool justCheckingForActivity)
            {
                return false;
            }

            internal static void Postfix(ref bool __result, ref TV __instance, SFarmer who, bool justCheckingForActivity)
            {
                CustomTVMod.checkForAction(__instance, who, justCheckingForActivity);
                __result = true;
            }
        }

    }
}
