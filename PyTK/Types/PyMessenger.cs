using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Newtonsoft.Json;
using StardewValley;

namespace PyTK.Types
{
    public class PyMessenger<T>
    {
        public string address;
        public XmlSerializer xmlSerializer;

        public PyMessenger(string address, XmlSerializer xmlSerializer = null)
        {
            this.address = address;
            this.xmlSerializer = xmlSerializer;
        }

        public void sendToFarmer(long farmer, SerializationType serializationType = SerializationType.PLAIN, params T[] dataSet)
        {
            foreach (T data in dataSet)
                PyNet.sendMessage(new MPMessage(address, Game1.player, serialize(serializationType,data), (int)serializationType, farmer));
        }

        internal object serialize(SerializationType serializationType, T data)
        {
            object objectData = data;

            if (serializationType == SerializationType.XML)
            {
                if (xmlSerializer == null)
                    xmlSerializer = new XmlSerializer(typeof(T));

                StringWriter writer = new StringWriter();
                xmlSerializer.Serialize(writer, data);
                objectData = writer.ToString();
            }
            else if (serializationType == SerializationType.JSON)
                objectData = JsonConvert.SerializeObject(data);

            return objectData;
        }

        public void send(SerializationType serializationType, params T[] dataSet)
        {
            sendToFarmer(-1, serializationType, dataSet);
        }

        public IEnumerable<T> receive(long fromFarmer = -1)
        {
            foreach (MPMessage msg in PyNet.getNewMessages(address, -1, fromFarmer))
                yield return (deserialize((SerializationType)msg.type, msg.message));
        }

        internal T deserialize(SerializationType type, object data)
        {
            if (type == (int)SerializationType.PLAIN)
                return (T) data;

            if (type == SerializationType.XML && xmlSerializer == null)
                xmlSerializer = new XmlSerializer(typeof(T));

            return (type == SerializationType.XML ? (T)xmlSerializer.Deserialize(new StringReader(data.ToString())) : JsonConvert.DeserializeObject<T>(data.ToString()));
        }

    }
}
