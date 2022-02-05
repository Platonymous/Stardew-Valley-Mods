using HarmonyLib;
using PyTK.Events;
using PyTK.Types;
using StardewValley;
using System.Reflection;

namespace PyTK.Overrides
{
    [HarmonyPatch]
    internal class OvBeforeSleep
    {

        internal static MethodInfo TargetMethod()
        {
            return AccessTools.Method(PyUtils.getTypeSDV("GameLocation"), "answerDialogue");
        }

        internal static void Prefix(GameLocation __instance, ref Response answer)
        {
            if (__instance.lastQuestionKey == null || answer == null || answer.responseKey == null)
                return;

            if (__instance.lastQuestionKey.ToLower() == "sleep" && answer.responseKey.ToLower() == "yes")
                PyTimeEvents.CallBeforeSleepEvents(null, new PyTimeEvents.EventArgsBeforeSleep(STime.CURRENT, false, ref answer));
        }
    }
 
}
