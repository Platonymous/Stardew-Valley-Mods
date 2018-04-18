using Harmony;
using PyTK.ConsoleCommands;
using PyTK.Events;
using PyTK.Types;
using StardewValley;
using System.Reflection;

namespace PyTK.Overrides
{
    [HarmonyPatch]
        internal class OvSleep
    {

        internal static MethodInfo TargetMethod()
        {
            return AccessTools.Method(PyUtils.getTypeSDV("GameLocation"), "answerDialogue");
        }

        internal static bool Prefix(GameLocation __instance, ref Response answer)
        {
            if (__instance.lastQuestionKey == null || answer == null || answer.responseKey == null)
                    return true;

            if (__instance.lastQuestionKey.ToLower() == "sleep" && answer.responseKey.ToLower() == "yes")
                PyTimeEvents.CallOnSleepEvents(null, new PyTimeEvents.EventArgsSleep(STime.CURRENT, false));

            return true;
        }
    }
}
