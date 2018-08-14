using Microsoft.Xna.Framework;
using StardewValley;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using Harmony;
using System.Reflection;
using StardewValley.Network;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using PyTK.Extensions;

namespace MuliplayerTweaks
{
    public class MultiplayerTweaksMod : Mod
    {
        internal static Config config;
        internal static IMonitor _monitor;
        internal static int maxDistance = 810000;
        internal static int overflow = 6;
        internal static Dictionary<long, OutgoingMessage> cache = new Dictionary<long, OutgoingMessage>();
        internal static Dictionary<long, int> peers = new Dictionary<long, int>();

        public override void Entry(IModHelper helper)
        {
            config = helper.ReadConfig<Config>();
            _monitor = Monitor;
            HarmonyInstance instance = HarmonyInstance.Create("Platonymous.MultiplayerTweaks");
            instance.PatchAll(Assembly.GetExecutingAssembly());

            if (config.LimitPositionSync)
            {
                maxDistance = config.LimitPositonSyncMaxDistance;
                overflow = config.LimitPositonSyncOverflow;
            }

            /*Keys.U.onPressed( () => 
            {
                Game1.game1.IsFixedTimeStep = false;
                Game1.debugMode = true;
                Helper.Reflection.GetField<DebugMetricsComponent>(Game1.game1, "_metrics").SetValue(new DebugMetricsComponent(Game1.game1));
                Program.releaseBuild = false;
            });*/
            }

        public static Type getTypeSDV(string type)
        {
            string prefix = "StardewValley.";
            Type defaulSDV = Type.GetType(prefix + type + ", Stardew Valley");

            if (defaulSDV != null)
                return defaulSDV;
            else
                return Type.GetType(prefix + type + ", StardewValley");

        }

        internal static double getDistance(Vector2 p1, Vector2 p2)
        {
            float distX = Math.Abs(p1.X - p2.X);
            float distY = Math.Abs(p1.Y - p2.Y);
            double dist = (distX * distX) + (distY * distY);
            return dist;
        }

        internal static bool skipMessage(long peerId, OutgoingMessage message)
        {
            if (message.MessageType != 0)
                return true;

            if (!peers.ContainsKey(peerId))
                peers.Add(peerId, overflow);

            Farmer compare = Game1.player;

            if (message.Data[0] is Byte[] b)
                compare = message.SourceFarmer;

            if (Game1.otherFarmers[peerId] == compare || (int.Parse(message.MessageType.ToString()) == 0 && (Game1.otherFarmers[peerId].currentLocation != compare.currentLocation || getDistance(new Vector2(compare.position.X, compare.position.Y), new Vector2(Game1.otherFarmers[peerId].position.X, Game1.otherFarmers[peerId].position.Y)) > maxDistance)))
            {
                peers[peerId]--;

                if (peers[peerId] > 0)
                    return true;
                else
                    return false;
            }
            else
            {
                peers[peerId] = overflow;
                return true;
            }
        }

        
    }

    [HarmonyPatch]
    internal class ServerSpeedFix
    {
        internal static MethodInfo TargetMethod()
        {
            return AccessTools.Method(MultiplayerTweaksMod.getTypeSDV("Network.GameServer"), "sendMessage", new[] { typeof(long), typeof(OutgoingMessage) });
        }

        internal static bool Prefix(GameServer __instance, long peerId, OutgoingMessage message)
        {
            if (MultiplayerTweaksMod.config.LimitPositionSync)
                return MultiplayerTweaksMod.skipMessage(peerId, message);
            else
                return true;
        }
    }


    [HarmonyPatch]
    internal class ServerLoadFix
    {
        internal static MethodInfo TargetMethod()
        {
            return AccessTools.Method(MultiplayerTweaksMod.getTypeSDV("Menus.CoopMenu"), "addSaveFiles");
        }

        internal static void Prefix(GameServer __instance, ref List<Farmer> files)
        {
            if (MultiplayerTweaksMod.config.UseAllSaves)
                files.ForEach(file => file.slotCanHost = true);
        }

    }
}
