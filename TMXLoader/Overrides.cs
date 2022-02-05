using HarmonyLib;
using StardewValley;
using StardewValley.Locations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TMXLoader
{
    class Overrides
    {
        internal static List<string> NPCs = new List<string>();

        [HarmonyPatch]
        internal class SpouseRoomTilesFix
        {
            internal static MethodInfo TargetMethod()
            {
               return PyTK.PyUtils.getTypeSDV("Locations.FarmHouse").GetMethod("loadSpouseRoom");
            }

            internal static bool Prefix(FarmHouse __instance)
            {
                if (__instance != null && __instance.owner != null && __instance.owner.getSpouse() != null && NPCs.Contains(__instance.owner.getSpouse().Name))
                    return false;

                return true;
            }
        }


        [HarmonyPatch]
        internal class PathFinderFix
        {
            internal static MethodInfo TargetMethod()
            {
                return PyTK.PyUtils.getTypeSDV("NPC").GetMethod("populateRoutesFromLocationToLocationList", BindingFlags.Public | BindingFlags.Static);
            }

            internal static void Prefix()
            {
                foreach (var edit in TMXLoaderMod.addedLocations)
                    if (Game1.getLocationFromName(edit.name) == null)
                        TMXLoaderMod.addLocation(edit);

            }

        }
    }
}
