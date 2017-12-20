using Harmony;
using StardewValley.BellsAndWhistles;

namespace Capitalism.Overrides
{
    [HarmonyPatch(typeof(MoneyDial), "draw")]
    public class MoneyDialFix
    {
        internal static void Prefix(ref MoneyDial __instance, ref int target)
        {
            CapitalismMod.counter = 0;
            target *= 10;
        }

        internal static void Postfix(ref MoneyDial __instance, ref int target)
        {
            CapitalismMod.counter = -1;
        }
    }


    
}
