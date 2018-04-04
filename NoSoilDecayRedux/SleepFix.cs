using Harmony;
using PyTK;
using StardewValley;
using System.Reflection;

namespace NoSoilDecayRedux
{
    [HarmonyPatch]
        internal class SleepFix
    {
        internal static MethodInfo TargetMethod()
        {
            return AccessTools.Method(PyUtils.getTypeSDV("GameLocation"), "answerDialogue");
        }

        internal static void Postfix(GameLocation __instance, ref Response answer)
        {
            if (__instance.lastQuestionKey == null || answer == null || answer.responseKey == null)
                    return;

            if (__instance.lastQuestionKey.ToLower() == "sleep" && answer.responseKey.ToLower() == "yes")
                NoSoilDecayReduxMod.saveHoeDirt();
        }
    }
}
