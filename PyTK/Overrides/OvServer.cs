using Harmony;
using PyTK.CustomElementHandler;
using StardewValley.Network;
using System.Reflection;
using System;
using StardewValley;
using StardewModdingAPI.Events;

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
                if (Game1.IsMasterGame)
                    SaveHandler.Replace();
            }

            internal static void Postfix()
            {
                if (Game1.IsMasterGame)
                    SaveHandler.RebuildFromActions();
            }

        }

            internal static bool gsskip = false;

        [HarmonyPatch]
        internal class ServerFix3
        {
            internal static MethodInfo TargetMethod()
            {
                return AccessTools.Method(PyUtils.getTypeSDV("Network.GameServer"), "processIncomingMessage");
            }

            internal static bool Prefix(GameServer __instance, IncomingMessage message)
            {
                if (gsskip)
                    return true;
                try
                {
                    if (message.MessageType == 99)
                        PyNet.receiveMessage(message);
                    else
                    {
                        gsskip = true;
                        __instance.processIncomingMessage(message);
                        gsskip = false;
                    }
                }
                catch(Exception e)
                {
                    PyTKMod._monitor.Log("Errot processing Message: Type:" + message.MessageType + " Data:" + message.Data, StardewModdingAPI.LogLevel.Error);
                    PyTKMod._monitor.Log(e.Message + ":" + e.StackTrace, StardewModdingAPI.LogLevel.Error);
                }

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
                if (Game1.IsMasterGame)
                    SaveHandler.Replace();
            }

            internal static void Postfix()
            {
                if (Game1.IsMasterGame)
                    SaveHandler.RebuildFromActions();
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
