using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony;
using System.Reflection;
using StardewValley;
using StardewModdingAPI;

namespace CustomFarmingRedux
{
    internal class OvCraftingRecipe
    {
        internal static IModHelper Helper = CustomFarmingReduxMod._helper;
        internal static IMonitor Monitor = CustomFarmingReduxMod._monitor;
        internal static string folder = CustomFarmingReduxMod.folder;
        internal static List<CustomMachineBlueprint> machines = CustomFarmingReduxMod.machines;

        [HarmonyPatch]
        internal class CraftingFix
        {
            internal static MethodInfo TargetMethod()
            {
                if (Type.GetType("StardewValley.CraftingRecipe, Stardew Valley") != null)
                    return AccessTools.Method(Type.GetType("StardewValley.CraftingRecipe, Stardew Valley"), "createItem");
                else
                    return AccessTools.Method(Type.GetType("StardewValley.CraftingRecipe, StardewValley"), "createItem");
            }

            internal static bool Prefix(CraftingRecipe __instance, ref Item __result)
            {
               if (machines.Find(m => m.fullid == __instance.name) is CustomMachineBlueprint blueprint)
                {
                    __result = new CustomMachine(blueprint);
                    return false;
                }

                return true;
            }
        }

    }
}
