using Harmony;
using PyTK.CustomElementHandler;
using StardewValley;
using StardewValley.Network;
using System.Linq;
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
                    SaveHandler.Replace();
            }

            internal static void Postfix()
            {
                    SaveHandler.Rebuild();
            }
        }

        [HarmonyPatch]
        internal class ServerFix3
        {
            internal static MethodInfo TargetMethod()
            {
                return AccessTools.Method(PyUtils.getTypeSDV("Network.GameServer"), "processIncomingMessage");
            }

            internal static bool Prefix(IncomingMessage message)
            {
                if (message.MessageType == 99)
                    PyNet.receiveMessage(message);
                else
                    return true;

                return false;
            }
        }

        [HarmonyPatch]
        internal class ServerFix4
        {
            internal static MethodInfo TargetMethod()
            {
                return AccessTools.Method(PyUtils.getTypeSDV("Network.GameServer"), "sendAvailableFarmhands");
            }

            internal static void Prefix()
            {
                SaveHandler.Replace();
            }

            internal static void Postfix()
            {
                SaveHandler.Rebuild();
            }
        }

        [HarmonyPatch]
        internal class ClientFix1
        {
            internal static MethodInfo TargetMethod()
            {
                return AccessTools.Method(PyUtils.getTypeSDV("Network.Client"), "processIncomingMessage");
            }

            internal static bool Prefix(IncomingMessage message)
            {
                if (message.MessageType == 99)
                    PyNet.receiveMessage(message);
                else
                    return true;

                return false;
            }
        }
    }
}
