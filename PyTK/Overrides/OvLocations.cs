using Harmony;
using StardewValley;
using System;
using SFarmer = StardewValley.Farmer;
using PyTK.Types;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using xTile.Dimensions;

namespace PyTK.Overrides
{
    internal class OvLocations
    {

        internal static Dictionary<string, Func<List<string>, SFarmer, Location, bool>> actions = new Dictionary<string, Func<List<string>, SFarmer, Location, bool>>(); 

        [HarmonyPatch]
        internal class ActionableFix
        {
            internal static MethodInfo TargetMethod()
            {
                if (Type.GetType("StardewValley.GameLocation, Stardew Valley") != null)
                    return AccessTools.Method(Type.GetType("StardewValley.GameLocation, Stardew Valley"), "isActionableTile");
                else
                    return AccessTools.Method(Type.GetType("StardewValley.Objects.TV, StardewValley"), "isActionableTile");
            }

            internal static void Postfix(GameLocation __instance, ref bool __result, int xTile, int yTile, SFarmer who)
            {
                if (__instance.doesTileHaveProperty(xTile, yTile, "Action", "Buildings") != null)
                {
                    List<string> prop = __instance.doesTileHaveProperty(xTile, yTile, "Action", "Buildings").Split(' ').ToList();

                    if (actions.ContainsKey(prop[0]))
                        Game1.isInspectionAtCurrentCursorTile = true;
                }
            }
        }

        [HarmonyPatch]
        internal class ActionFix
        {
            internal static MethodInfo TargetMethod()
            {
                if (Type.GetType("StardewValley.GameLocation, Stardew Valley") != null)
                    return AccessTools.Method(Type.GetType("StardewValley.GameLocation, Stardew Valley"), "performAction");
                else
                    return AccessTools.Method(Type.GetType("StardewValley.Objects.TV, StardewValley"), "performAction");
            }

            internal static bool Prefix(GameLocation __instance, ref bool __result, string action, SFarmer who, Location tileLocation)
            {
                List<string> prop = action.Split(' ').ToList();
                if (actions.ContainsKey(prop[0]))
                {
                    __result = actions[prop[0]].Invoke(prop, who, tileLocation);
                    return false;
                }
                return true;
            }
        }
    }
}
