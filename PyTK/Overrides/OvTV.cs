using HarmonyLib;
using StardewValley.Objects;
using System.Reflection;
using SFarmer = StardewValley.Farmer;
using PyTK.CustomTV;
using StardewValley;

namespace PyTK.Overrides
{
    internal class OvTV
    {
       [HarmonyPatch]
        internal class TVFix
        {
            internal static MethodInfo TargetMethod()
            {
                    return AccessTools.Method(PyUtils.getTypeSDV("Objects.TV"), "checkForAction");
            }

            internal static bool Prefix(ref TV __instance, SFarmer who, bool justCheckingForActivity)
            {
                if (CustomTVMod.channels.Count < 6 && !CustomTVMod.changed)
                    return true;

                if (Game1.Date.Season == "fall" && Game1.Date.DayOfMonth == 26 && (Game1.stats.getStat("childrenTurnedToDoves") > 0U && !who.mailReceived.Contains("cursed_doll")))
                    return true;

                return false;
            }

            internal static void Postfix(ref bool __result, ref TV __instance, SFarmer who, bool justCheckingForActivity)
            {
                if (CustomTVMod.channels.Count < 6 && !CustomTVMod.changed)
                    return;

                if (Game1.Date.Season == "fall" && Game1.Date.DayOfMonth == 26 && (Game1.stats.getStat("childrenTurnedToDoves") > 0U && !who.mailReceived.Contains("cursed_doll")))
                    return;

                CustomTVMod.checkForAction(__instance, who, justCheckingForActivity);
                __result = true;
            }
        }

    }
}
