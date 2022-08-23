using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PyTK.Types;
using StardewModdingAPI;
using System.Threading;
using StardewValley;
using System.Xml.Serialization;
using System.IO;
using Newtonsoft.Json;
using PyTK.ContentSync;
using xTile.Tiles;
using Microsoft.Xna.Framework.Graphics;
using xTile;
using System.IO.Compression;

namespace PyTK
{

    public class PyNet
    {
        internal static IModHelper Helper { get; } = PyTKMod._helper;
        internal static IMonitor Monitor { get; } = PyTKMod._monitor;
        internal static Dictionary<string, List<MPMessage>> messages = new Dictionary<string, List<MPMessage>>();
        internal static Random random { get; } = new Random();
        
        public static void sendMessage(string address, object message)
        {
            sendMessage(new MPMessage(address, Game1.player, message));
        }

        public static void sendMessage(MPMessage msg)
        {
            Helper.Multiplayer.SendMessage<MPMessageSMAPI>(new MPMessageSMAPI((int) msg.dataType, msg.type, msg.message), msg.address, new[] { Helper.Multiplayer.ModID }, msg.receiver != -1 ? new[] { msg.receiver } : null);
        }
       
        internal static void Multiplayer_ModMessageReceived(object sender, StardewModdingAPI.Events.ModMessageReceivedEventArgs e)
        {
            MPMessageSMAPI message = e.ReadAs<MPMessageSMAPI>();

            if (!messages.ContainsKey(e.Type))
                messages.Add(e.Type, new List<MPMessage>());

            object data = null;
            switch (message.DataType)
            {
                case 0: data = message.AsInt; break;
                case 1: data = message.AsString; break;
                case 2: data = message.AsBool; break;
                case 3: data = message.AsLong; break;
                case 4: data = message.AsDouble; break;
                default: data = message.AsString; break;
            }
            messages[e.Type].Add(new MPMessage(e.Type, Game1.getFarmer(e.FromPlayerID), data, message.MessageType, Game1.player.UniqueMultiplayerID));
        }

        public static IEnumerable<MPMessage> getNewMessages(string address, int type = -1, long fromFarmer = -1)
        {
            if (!messages.ContainsKey(address))
                messages.Add(address, new List<MPMessage>());

            List<MPMessage> msgs = new List<MPMessage>(messages[address]);

            foreach (MPMessage msg in msgs)
            {
                if ((type == -1 || msg.type == type) && (fromFarmer == -1 || fromFarmer == msg.sender.UniqueMultiplayerID))
                {
                    messages[address].Remove(msg);
                    yield return msg;
                }
            }
        }

        public static void sendRequestToAllFarmers<T>(string address, object request, Action<T> callback, SerializationType serializationType = SerializationType.PLAIN, int timeout = 500, XmlSerializer xmlSerializer = null)
        {
            sendRequestToFarmer(address, request, -1, callback, serializationType, timeout, xmlSerializer);
        }

        public static void sendDataToFarmer(string address, object data, Farmer farmer, SerializationType serializationType = SerializationType.PLAIN, XmlSerializer xmlSerializer = null)
        {
            sendRequestToFarmer<bool>(address, data, farmer.UniqueMultiplayerID, null, serializationType, -1, xmlSerializer);
        }

        public static void sendDataToFarmer(string address, object data, long uniqueMultiplayerId, SerializationType serializationType = SerializationType.PLAIN, XmlSerializer xmlSerializer = null)
        {
            sendRequestToFarmer<bool>(address, data, uniqueMultiplayerId, null, serializationType, -1, xmlSerializer);
        }

        public static Task<T> sendRequestToFarmer<T>(string address, object request, Farmer farmer, Action<T> callback = null, SerializationType serializationType = SerializationType.PLAIN, int timeout = 500, XmlSerializer xmlSerializer = null)
        {
            return sendRequestToFarmer(address, request, farmer.UniqueMultiplayerID, callback, serializationType, timeout, xmlSerializer);
        }

        public static Task<T> sendRequestToFarmer<T>(string address, object request, long uniqueMultiplayerId, Action<T> callback = null, SerializationType serializationType = SerializationType.PLAIN, int timeout = 500, XmlSerializer xmlSerializer = null)
        {
           return Task.Run(() =>
           {
               long toFarmer = uniqueMultiplayerId;

               if (xmlSerializer == null)
                   xmlSerializer = new XmlSerializer(typeof(T));

               object objectData = request;

               if (serializationType == SerializationType.XML)
               {
                   StringWriter writer = new StringWriter();
                   xmlSerializer.Serialize(writer, request);
                   objectData = writer.ToString();
               }
               else if (serializationType == SerializationType.JSON)
                   objectData = JsonConvert.SerializeObject(request);
               int id = Guid.NewGuid().GetHashCode();
               string returnAddress = address + "." + id;
               PyMessenger<T> messenger = new PyMessenger<T>(returnAddress, null);

               int run = 0;
               bool received = false;
               T result = default(T);

               EventHandler<StardewModdingAPI.Events.ModMessageReceivedEventArgs> handler = (s, e) => checkForMessage(messenger, ref result, ref received, ref run);
               Helper.Events.Multiplayer.ModMessageReceived += handler;

               sendMessage(new MPMessage(address, Game1.player, objectData, id, toFarmer));
               if (timeout > 0)
                   while (!received && run < 100 && timeout > 0)
                   {
                       timeout--;
                       Thread.Sleep(10);
                   }

               Helper.Events.Multiplayer.ModMessageReceived -= handler;

               callback?.Invoke((T)result);

               return((T)result);
           });
        }

        private static void checkForMessage<T>(PyMessenger<T> messenger, ref T result, ref bool received, ref int run)
        {
           foreach(T msg in messenger.receive())
            {
                result = msg;
                received = true;
                break;
            }

            if (received)
                messages.Remove(messenger.address);

            run++;
        }

        public static void requestContent<T>(string assetName, Farmer fromFarmer, Action<T> callback, int timeout = 1000)
        {
            ContentSyncHandler.requestContent<T>(assetName, fromFarmer, callback, timeout);
        }

        public static void requestGameContent<T>(string assetName, Farmer fromFarmer, Action<T> callback, int timeout = 1000)
        {
            ContentSyncHandler.requestGameContent<T>(assetName, fromFarmer, callback, timeout);
        }

        public static void sendContent<T>(string assetName, T content, Farmer toFarmer, Action<bool> callback, int timeout = 1000)
        {
            ContentSyncHandler.sendContent<T>(assetName, content, toFarmer, callback, timeout);
        }

        public static void sendGameContent<T>(string assetName, T content, Farmer toFarmer, Action<bool> callback, int timeout = 1000)
        {
            ContentSyncHandler.sendGameContent<T>(assetName, content, toFarmer, callback, timeout);
        }

        public static void sendGameContent<T>(string[] assetName, T content, Farmer toFarmer, Action<bool> callback, int timeout = 1000)
        {
            ContentSyncHandler.sendGameContent<T>(assetName, content, toFarmer, callback, timeout);
        }

        public static void syncLocationMapToAll(string location)
        {
            foreach (Farmer farmer in Game1.otherFarmers.Values)
                syncLocationMapToFarmer(location, farmer);
        }

        public static void syncLocationMapToFarmer(string location, Farmer farmer)
        {
            syncMap(Game1.getLocationFromName(location).map, location, farmer);
        }

        public static void syncMap(Map map, string mapName, Farmer farmer)
        {
            Dictionary<TileSheet, Texture2D> tilesheets = Helper.Reflection.GetField<Dictionary<TileSheet, Texture2D>>(Game1.mapDisplayDevice, "m_tileSheetTextures").GetValue();
            string[] seasons = new[] { "spring", "summer", "fall", "winter" };
            PyNet.sendGameContent<Map>(Path.Combine("Maps", mapName), map, farmer,null);

            foreach (TileSheet t in map.TileSheets)
                {
                    if (t.Id.StartsWith("z"))
                    {
                        Texture2D texture = null;

                        try
                        {
                            texture = Helper.GameContent.Load<Texture2D>(t.ImageSource);
                        }
                        catch
                        {
                            try
                            {
                                texture = Helper.GameContent.Load<Texture2D>($"Maps/{t.ImageSource}");
                            }
                            catch
                            {

                            }
                        }

                        string filename = Path.GetFileName(t.ImageSource);
                        PyNet.sendGameContent(new[] { filename, Path.Combine("Maps", filename) }, texture, farmer, (b) => Monitor.Log("Syncing " + t.ImageSource + " to " + farmer.Name + ": " + (b ? "successful" : "failed"), b ? LogLevel.Info : LogLevel.Warn));

                        foreach (string season in seasons)
                            if (season is string cSeason && filename.Contains(season))
                            {
                                foreach (string s in seasons.Where(cs => cs != cSeason))
                                {
                                    Texture2D seasonTexture = null;
                                    string sFilename = filename.Replace(cSeason, s);
                                    string sFilenameMaps = Path.Combine("Maps", sFilename);
                                    try
                                    {
                                        seasonTexture = Helper.GameContent.Load<Texture2D>(sFilename);
                                    }
                                    catch
                                    {
                                        try
                                        {
                                            seasonTexture = Helper.GameContent.Load<Texture2D>(sFilenameMaps);
                                        }
                                        catch
                                        {

                                        }
                                    }

                                    if (seasonTexture is Texture2D sTex)
                                    {
                                        Monitor.Log("Syncing Texture " + sFilename + " with " + farmer.Name, LogLevel.Info);
                                        PyNet.sendGameContent(new[] { sFilename, sFilenameMaps }, sTex, farmer, (b) => Monitor.Log("Syncing " + sFilename + " to " + farmer.Name + ": " + (b ? "successful" : "failed"), b ? LogLevel.Info : LogLevel.Warn));
                                    }
                                }
                                break;
                            }
                    }
                }
        }

        public static byte[] DecompressBytes(byte[] data)
        {
            return Decompress(data);
        }

        public static byte[] CompressStringToBytes(string str)
        {
            return Compress(System.Text.Encoding.UTF8.GetBytes(str));
        }

        public static string DecompressString(byte[] str)
        {
            return System.Text.Encoding.UTF8.GetString(Decompress(str));
        }

        public static string CompressBytes(byte[] buffer)
        {
            return Convert.ToBase64String(Compress(buffer));
        }

        public static byte[] Compress(byte[] buffer)
        {
            using (MemoryStream decompressed = new MemoryStream(buffer))
            using (MemoryStream compressed = new MemoryStream())
            using (GZipStream gzip = new GZipStream(compressed, CompressionMode.Compress))
            {
                decompressed.CopyTo(gzip);
                gzip.Close();
                return compressed.ToArray();
            }
        }

        public static byte[] Decompress(byte[] buffer)
        {
            using (MemoryStream decompressed = new MemoryStream())
            using (MemoryStream compressed = new MemoryStream(buffer))
            using (GZipStream gzip = new GZipStream(compressed, CompressionMode.Decompress))
            {
                gzip.CopyTo(decompressed);
                return decompressed.ToArray();
            }
        }

        public static byte[] DecompressBytes(string data)
        {
            return Decompress(Convert.FromBase64String(data));
        }

        public static string CompressString(string str)
        {
            return CompressBytes(System.Text.Encoding.UTF8.GetBytes(str));
        }

        public static string DecompressString(string str)
        {
            return System.Text.Encoding.UTF8.GetString(Decompress(Convert.FromBase64String(str)));
        }

        public void WarpFarmer(Farmer farmer, string location, int x, int y, bool isStructure = false, int facingAfterWarp = -1)
        {
            Task.Run(() =>
           {
               sendRequestToFarmer<bool>("PyTK.WarpFarmer", new WarpRequest(farmer, location, x, y, isStructure, facingAfterWarp), farmer, (b) => Monitor.Log("Warping " + farmer.Name + " " + (b ? "was successful" : "failed"), b ? LogLevel.Info : LogLevel.Error), SerializationType.JSON);
           });
        }
    }
}
