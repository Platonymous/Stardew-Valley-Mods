using Harmony;
using PyTK.CustomElementHandler;
using StardewValley;
using System.Reflection;

namespace PyTK.Overrides
{
    internal class OvServer
    {
        [HarmonyPatch]
        internal class ServerFix
        {
            internal static MethodInfo TargetMethod()
            {
                return AccessTools.Method(PyUtils.getTypeSDV("Network.GameServer"), "sendServerIntroduction");
            }

            internal static void Prefix()
            {
                    SaveHandler.Replace();
            }

            internal static void Postfix()
            {
                if (Game1.IsClient || Game1.numberOfPlayers() < 2)
                    SaveHandler.Rebuild();
            }
        }

        [HarmonyPatch]
        internal class ServerFix2
        {
            internal static MethodInfo TargetMethod()
            {
                return AccessTools.Method(PyUtils.getTypeSDV("Network.GameServer"), "playerDisconnected");
            }

            internal static void Prefix()
            {
                if (Game1.IsServer)
                    SaveHandler.Replace();
            }

            internal static void Postfix()
            {
                if (Game1.IsServer)
                    SaveHandler.Rebuild();
            }
        }
    }
}
