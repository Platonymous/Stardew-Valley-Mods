using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PyTK.Types;
using StardewModdingAPI;
using System.Threading;
using StardewValley;
using StardewValley.Network;
using System.Xml.Serialization;
using System.IO;
using Newtonsoft.Json;
using PyTK.ContentSync;
using xTile.Tiles;
using Microsoft.Xna.Framework.Graphics;
using xTile;
using Netcode;

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
            if (Game1.IsServer)
                foreach (long key in Game1.otherFarmers.Keys)
                {
                    if (key != msg.sender.UniqueMultiplayerID)
                        if (msg.receiver == -1 || msg.receiver == key)
                            Game1.server.sendMessage(key, 99, msg.sender, (Int64)msg.receiver, (Int16)msg.type, msg.address, (Int16)msg.dataType, msg.message);
                }
            else
                Game1.client.sendMessage(new OutgoingMessage(99, msg.sender, (Int64)msg.receiver, (Int16)msg.type, msg.address, (Int16)msg.dataType, msg.message));
        }

        public static void receiveMessage(IncomingMessage inc)
        {
            long receiver = inc.Reader.ReadInt64();
            int type = inc.Reader.ReadInt16();
            string address = inc.Reader.ReadString();
            MPDataType dataType = (MPDataType)inc.Reader.ReadInt16();
            object data = null;
            switch (dataType)
            {
                case MPDataType.STRING: data = inc.Reader.ReadString(); break;
                case MPDataType.INT: data = inc.Reader.ReadInt32(); break;
                case MPDataType.BOOL: data = inc.Reader.ReadBoolean(); break;
                case MPDataType.DOUBLE: data = inc.Reader.ReadDouble(); break;
                case MPDataType.LONG: data = inc.Reader.ReadInt64(); break;
                default: data = inc.Reader.ReadString(); break;
            }

            MPMessage message = new MPMessage(address, inc.SourceFarmer, data, type, receiver);

            if (Game1.IsServer && receiver != Game1.player.UniqueMultiplayerID)
                sendMessage(message);

            if (receiver == -1 || receiver == Game1.player.UniqueMultiplayerID)
            {
                if (!messages.ContainsKey(address))
                    messages.Add(address, new List<MPMessage>());

                messages[address].Add(message);
            }
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

        public static void sendRequestToAllFarmers<T>(string address, object request, Action<T> callback, SerializationType serializationType = SerializationType.PLAIN, int timeout = 1000, XmlSerializer xmlSerializer = null)
        {
            foreach (Farmer farmer in Game1.otherFarmers.Values.Where(f => f.isActive() &&  f != Game1.player))
                Task.Run(() => sendRequestToFarmer(address, request, farmer, callback, serializationType, timeout, xmlSerializer));
        }

        public static async Task<T> sendRequestToFarmer<T>(string address, object request, Farmer farmer, Action<T> callback = null, SerializationType serializationType = SerializationType.PLAIN, int timeout = 500, XmlSerializer xmlSerializer = null)
        {
                long fromFarmer = farmer.UniqueMultiplayerID;

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

                Int16 id = (Int16)random.Next(Int16.MinValue, Int16.MaxValue);
                string returnAddress = address + "." + id;
                PyMessenger<T> messenger = new PyMessenger<T>(returnAddress);
                sendMessage(new MPMessage(address, Game1.player, objectData, id, fromFarmer));

                object result = await Task.Run(() =>
                {
                    while (true)
                    {
                        List<T> msgs = new List<T>(messenger.receive());
                        if (msgs.Count() > 0)
                        {
                            messages.Remove(returnAddress);
                            return msgs[0];
                        }

                        timeout--;

                        if (timeout < 0)
                            return default(T);
                        Thread.Sleep(1);
                    }
                });
                callback?.Invoke((T)result);
                return (T)result;
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
            Monitor.Log("Syncing Map " + mapName + " with " + farmer.Name, LogLevel.Info);
            PyNet.sendGameContent<Map>(Path.Combine("Maps", mapName), map, farmer, (b) => Monitor.Log("Syncing " + mapName + " to " + farmer.Name + ": " + (b ? "successful" : "failed"), b ? LogLevel.Info : LogLevel.Warn));

            foreach (TileSheet t in map.TileSheets)
                {
                    if (t.Id.StartsWith("z"))
                    {
                        Texture2D texture = null;

                        try
                        {
                            texture = Helper.Content.Load<Texture2D>(t.ImageSource, ContentSource.GameContent);
                        }
                        catch
                        {
                            try
                            {
                                texture = Helper.Content.Load<Texture2D>(Path.Combine("Maps", t.ImageSource), ContentSource.GameContent);
                            }
                            catch
                            {

                            }
                        }

                        if (texture == null)
                        {
                            Monitor.Log("Syncing Texture " + t.ImageSource + " failed. Could not load file.", LogLevel.Error);
                        }

                        string filename = Path.GetFileName(t.ImageSource);
                        Monitor.Log("Syncing Texture " + filename + " with " + farmer.Name, LogLevel.Info);
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
                                        seasonTexture = Helper.Content.Load<Texture2D>(sFilename, ContentSource.GameContent);
                                    }
                                    catch
                                    {
                                        try
                                        {
                                            seasonTexture = Helper.Content.Load<Texture2D>(sFilenameMaps, ContentSource.GameContent);
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
            return Ionic.Zlib.GZipStream.UncompressBuffer(data);
        }

        public static byte[] CompressStringToBytes(string str)
        {
            return Ionic.Zlib.GZipStream.CompressString(str);
        }

        public static string DecompressString(byte[] str)
        {
            return Ionic.Zlib.GZipStream.UncompressString(str);
        }

        public static string CompressBytes(byte[] buffer)
        {
            return Convert.ToBase64String(Ionic.Zlib.GZipStream.CompressBuffer(buffer));
        }

        public static byte[] DecompressBytes(string data)
        {
            return Ionic.Zlib.GZipStream.UncompressBuffer(Convert.FromBase64String(data));
        }

        public static string CompressString(string str)
        {
            return Convert.ToBase64String(Ionic.Zlib.GZipStream.CompressString(str));
        }

        public static string DecompressString(string str)
        {
            return Ionic.Zlib.GZipStream.UncompressString(Convert.FromBase64String(str));
        }

        public void WarpFarmer(Farmer farmer, string location, int x, int y, bool isStructure = false, int facingAfterWarp = -1)
        {
            Task.Run(async () =>
           {
               await sendRequestToFarmer<bool>("PyTK.WarpFarmer", new WarpRequest(farmer, location, x, y, isStructure, facingAfterWarp), farmer, (b) => Monitor.Log("Warping " + farmer.Name + " " + (b ? "was successful" : "failed"), b ? LogLevel.Info : LogLevel.Error), SerializationType.JSON);
           });
        }
    }
}
