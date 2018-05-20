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
                            Game1.server.sendMessage(key, 99, msg.sender, (Int64) msg.receiver, (Int16) msg.type, msg.address,  (Int16) msg.dataType, msg.message);
                }
            else
                Game1.client.sendMessage(new OutgoingMessage(99, msg.sender, (Int64) msg.receiver, (Int16) msg.type, msg.address, (Int16) msg.dataType, msg.message));
        }
 
        public static void receiveMessage(IncomingMessage inc)
        {
            long receiver = inc.Reader.ReadInt64();
            int type = inc.Reader.ReadInt16();
            string address = inc.Reader.ReadString();
            MPDataType dataType = (MPDataType) inc.Reader.ReadInt16();
            object data = null;
            switch (dataType)
            {
                case MPDataType.STRING: data = inc.Reader.ReadString(); break;
                case MPDataType.INT: data = inc.Reader.ReadInt32(); break;
                case MPDataType.BOOL: data = inc.Reader.ReadBoolean(); break;
                case MPDataType.DOUBLE: data = inc.Reader.ReadDouble(); break;
                case MPDataType.LONG: data = inc.Reader.ReadInt64(); break;
                default: data = inc.Reader.ReadString();break;
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
        
        public static void sendRequestToAllFarmers<T>(string address, object request, Action<T> callback, SerializationType serializationType = SerializationType.PLAIN, int timeout = 10000, XmlSerializer xmlSerializer = null)
        {
            foreach (Farmer farmer in Game1.otherFarmers.Values)
                Task.Run(()=> sendRequestToFarmer(address, request, farmer, callback, serializationType, timeout, xmlSerializer));
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

            Int16 id = (Int16) random.Next(Int16.MinValue, Int16.MaxValue);
            string returnAddress = address + "." + id;
            PyMessenger<T> messenger = new PyMessenger<T>(returnAddress);
            sendMessage(new MPMessage(address, Game1.player, objectData, id, fromFarmer));

            object result = await Task.Run(() =>
            {
                while (true)
                {
                    List<T> msgs = new List<T>(messenger.receive(fromFarmer));
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
    }
}
