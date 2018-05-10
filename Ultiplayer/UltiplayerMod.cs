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
//using PyTK;
//using Microsoft.Xna.Framework.Input;
using StardewModdingAPI.Events;

namespace Ultiplayer
{
    public class UltiplayerMod : Mod
    {
        internal static IModHelper help;
        internal static IMonitor mon;
        public static Random rnd;
        public static List<NetRef<Farmer>> farmers;
        public static string farmhandDirectory;

        public override void Entry(IModHelper helper)
        {
            help = Helper;
            mon = Monitor;
            rnd = new Random();
            farmhandDirectory = Path.Combine(help.DirectoryPath, "farmhands");
            HarmonyInstance instance = HarmonyInstance.Create("Platonymous.Ultiplayer");
            instance.PatchAll(Assembly.GetExecutingAssembly());
            farmers = new List<NetRef<Farmer>>();
           
            /* 
            
            Keys.K.onPressed(() =>
            {
                mon.Log("Checking farmers:" + farmers.Count);
                foreach (NetRef<Farmer> f in farmers)
                {
                    mon.Log(f.Value.Name + ":" + f.Value.UniqueMultiplayerID);
                }
            });

           Keys.L.onPressed(() =>
            {
                mon.Log("Checking Multiplayer:" + Game1.otherFarmers.Count);
                
                foreach (Farmer f in Game1.otherFarmers.Values)
                {
                    mon.Log(f.Name + ":" + f.UniqueMultiplayerID);
                }
            });
            
             */

            TimeEvents.AfterDayStarted += (s,e) => SyncFarmhands();
            SaveEvents.AfterLoad += (s, e) => LoadFarmhands();
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

        private static void LoadFarmhands()
        {
            string[] files = Directory.GetFiles(farmhandDirectory, "*." + Game1.uniqueIDForThisGame + ".xml", SearchOption.TopDirectoryOnly);
            foreach (string file in files)
            {
                FileStream fs = new FileStream(file, FileMode.Open);
                Farmer f = (Farmer) SaveGame.farmerSerializer.Deserialize(fs);
                farmers.Add(new NetRef<Farmer>(f));
            }
        }


        private static void SyncFarmhands(long id = 0)
        {
            mon.Log("SyncFarmhands");
            foreach (Farmer f in Game1.otherFarmers.Values)
                if (farmers.Find(fh => (id == 0 || f.UniqueMultiplayerID == id) && fh.Value.UniqueMultiplayerID == f.UniqueMultiplayerID) is NetRef<Farmer> nfh)
                {
                    nfh.Value = f;
                    string path = Path.Combine(farmhandDirectory, f.Name + "_" + f.UniqueMultiplayerID + "." + Game1.uniqueIDForThisGame + ".xml");
                    FileStream fs = new FileStream(path, FileMode.Create);
                    SaveGame.farmerSerializer.Serialize(fs, f);
                }
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
                mon.Log("sendAvailableFarmhands:" + userID + ":" + sendMessage.ToString());
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
                mon.Log("checkFarmhandRequest:" + userID + ":" + farmer.Value.Name);               
                long id = farmer.Value.UniqueMultiplayerID;
                approve();
                Multiplayer multiplayer = (Multiplayer) typeof(Game1).GetField("multiplayer", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
                farmer.Value.currentLocation = Game1.getLocationFromName("BusStop");
                farmer.Value.Position = new Vector2(704f, 704f);
                multiplayer.addPlayer(farmer);
                multiplayer.broadcastPlayerIntroduction(farmer);
                __instance.sendServerIntroduction(id);
                __instance.updateLobbyData();
                return false;
            }

        }

        [HarmonyPatch]
        internal class ServerFix3
        {
            internal static MethodInfo TargetMethod()
            {
                return AccessTools.Method(getTypeSDV("Menus.CoopMenu"), "addSaveFiles");
            }

            internal static void Prefix(GameServer __instance, ref List<Farmer> files)
            {
                files.ForEach(file => file.slotCanHost = true);
            }

        }

        [HarmonyPatch]
        internal class ServerFix4
        {
            internal static MethodInfo TargetMethod()
            {
                return AccessTools.Method(getTypeSDV("Multiplayer"), "removeDisconnectedFarmers");
            }

            internal static void Prefix(Multiplayer __instance)
            {
                List<long> disc = help.Reflection.GetField<List<long>>(__instance, "disconnectingFarmers").GetValue();
                foreach (long id in disc)
                    SyncFarmhands(id);
            }
        }

            public static NetRef<Farmer> getNewFarmHand()
        {
            NetRef<Farmer> farmhand = new NetRef<Farmer>();
            farmhand.Value = new Farmer(new FarmerSprite((string)null), new Vector2(0.0f, 0.0f), 1, "", Farmer.initialTools(), true);
            farmhand.Value.UniqueMultiplayerID = Utility.RandomLong(rnd);
            farmhand.Value.questLog.Add((Quest)(Quest.getQuestFromId(9) as SocializeQuest));
            farmhand.Value.farmName.Value = Game1.MasterPlayer.farmName.Value;
            farmhand.Value.homeLocation.Value = "BusStop";
            farmhand.Value.currentLocation = Game1.getLocationFromName("BusStop");
            farmhand.Value.Position = new Vector2(704f, 704f);
            return farmhand;
        }

        public static bool authCheck(string userID, Farmer farmhand)
        {
            if (!Game1.options.enableFarmhandCreation && !farmhand.isCustomized.Value)
                return false;
            if (!(userID == "") && !(farmhand.userID.Value == ""))
                return farmhand.userID.Value == userID;
            return true;
        }


        public static void sendAvailableFarmhands(string userID, Action<OutgoingMessage> sendMessage)
        {
            mon.Log("sendAvailableFarmhands:" + userID);

            Multiplayer multiplayer = (Multiplayer)typeof(Game1).GetField("multiplayer", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
            List<NetRef<Farmer>> netRefList = new List<NetRef<Farmer>>();

            foreach (NetRef<Farmer> f in farmers)
                if ((!f.Value.isActive() || multiplayer.isDisconnecting(f.Value.UniqueMultiplayerID)) && authCheck(userID, f.Value))
                {
                    f.Value.currentLocation = Game1.getLocationFromName("BusStop");
                    f.Value.Position = new Vector2(704f, 704f);
                    netRefList.Add(f);
                }

            mon.Log("Found:" + netRefList.Count);

            if (netRefList.Count < 1)
            {
                mon.Log("Creating New");
                NetRef<Farmer> newF = getNewFarmHand();
                farmers.Add(newF);
                netRefList.Add(newF);
            }

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
                        }catch(Exception e)
                        {
                            mon.Log(e.Message + ":" + e.StackTrace);
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
