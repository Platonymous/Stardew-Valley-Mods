using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using Harmony;
using Microsoft.Xna.Framework;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Network;
using StardewValley.Quests;

namespace Ultiplayer
{
    public class UltiplayerMod : Mod
    {
        internal static IModHelper help;
        internal static IMonitor mon;
        public static Random rnd;

        public override void Entry(IModHelper helper)
        {
            help = Helper;
            mon = Monitor;
            rnd = new Random();
            HarmonyInstance instance = HarmonyInstance.Create("Platonymous.Ultiplayer");
            instance.PatchAll(Assembly.GetExecutingAssembly());
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

        [HarmonyPatch]
        internal class ServerFix
        {
            internal static MethodInfo TargetMethod()
            {
                return AccessTools.Method(getTypeSDV("Network.GameServer"), "sendAvailableFarmhands");
            }

            internal static bool Prefix(GameServer __instance, string userID, Action<OutgoingMessage> sendMessage)
            {
                mon.Log("test", LogLevel.Trace);
                sendAvailableFarmhands(userID, sendMessage);
                return false;
             }

        }

        [HarmonyPatch]
        internal class ServerFix2
        {
            internal static MethodInfo TargetMethod()
            {
                return AccessTools.Method(getTypeSDV("Network.GameServer"), "checkFarmhandRequest");
            }

            internal static bool Prefix(GameServer __instance, string userID, NetFarmerRoot farmer, Action<OutgoingMessage> sendMessage, Action approve)
            {
                long id = farmer.Value.UniqueMultiplayerID;
                approve();
                Multiplayer multiplayer = (Multiplayer) typeof(Game1).GetField("multiplayer", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
                multiplayer.addPlayer(farmer);
                multiplayer.broadcastPlayerIntroduction(farmer);
                __instance.sendServerIntroduction(id);
                __instance.updateLobbyData();
                return false;
            }

        }

        

        public static NetRef<Farmer> getNewFarmHand()
        {
            NetRef<Farmer> farmhand = new NetRef<Farmer>();
            farmhand.Value = new Farmer(new FarmerSprite((string)null), new Vector2(0.0f, 0.0f), 1, "", Farmer.initialTools(), true);
            farmhand.Value.UniqueMultiplayerID = Utility.RandomLong(rnd);
            farmhand.Value.questLog.Add((Quest)(Quest.getQuestFromId(9) as SocializeQuest));
            farmhand.Value.farmName.Value = Game1.MasterPlayer.farmName.Value;
            farmhand.Value.homeLocation.Value = "FarmHouse";
            farmhand.Value.currentLocation = Game1.getLocationFromName("FarmHouse");
            farmhand.Value.Position = new Vector2(640f, 320f);
            return farmhand;
        }

        public static void sendAvailableFarmhands(string userID, Action<OutgoingMessage> sendMessage)
        {
            List<NetRef<Farmer>> netRefList = new List<NetRef<Farmer>>();
                    netRefList.Add(getNewFarmHand());
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter((Stream)memoryStream))
                {
                    writer.Write(Game1.year);
                    writer.Write(Utility.getSeasonNumber(Game1.currentSeason));
                    writer.Write(Game1.dayOfMonth);
                    writer.Write((byte)netRefList.Count);
                    foreach (NetRef<Farmer> netRef in netRefList)
                    {
                        try
                        {
                            netRef.Serializer = SaveGame.farmerSerializer;
                            netRef.WriteFull(writer);
                        }
                        finally
                        {
                            netRef.Serializer = (XmlSerializer)null;
                        }
                    }
                    memoryStream.Seek(0L, SeekOrigin.Begin);
                    sendMessage(new OutgoingMessage((byte)9, Game1.player, new object[1]
                    {
            (object) memoryStream.ToArray()
                    }));
                }
            }
        }
    }
}
