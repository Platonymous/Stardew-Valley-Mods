using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using StardewModdingAPI.Events;
using System.Threading.Tasks;
using StardewValley;
using Newtonsoft.Json;

namespace TMXLoader
{

    public class PyReceiver<TIn> : IPyResponder
    {
        public string address { get; set; }
        public int interval;
        public XmlSerializer xmlSerializer;
        public Action<TIn> requestHandler;
        public SerializationType serializationType;
        public SerializationType requestSerialization;

        public PyReceiver(string address, Action<TIn> requestHandler, int interval = 1, SerializationType requestSerialization = SerializationType.PLAIN, XmlSerializer xmlSerializer = null)
        {
            this.requestSerialization = requestSerialization;
            this.interval = interval;
            this.requestHandler = requestHandler;
            this.address = address;
            this.xmlSerializer = xmlSerializer;
        }

        public void start()
        {
            TMXLoaderMod.helper.Events.GameLoop.UpdateTicked += checkForRequests;
        }

        public void stop()
        {
            TMXLoaderMod.helper.Events.GameLoop.UpdateTicked -= checkForRequests;
        }

        private void checkForRequests(object sender, UpdateTickedEventArgs e)
        {
            if (!Game1.IsMultiplayer)
                return;

            if (!e.IsMultipleOf((uint)interval))
                return;

            var messages = receive();

            foreach (MPMessage request in messages)
                Task.Run(() => { requestHandler(deserialize(requestSerialization, request.message)); ; });
        }

        private TIn deserialize(SerializationType type, object data)
        {
            if (type == (int)SerializationType.PLAIN)
                return (TIn)data;

            if (type == SerializationType.XML && xmlSerializer == null)
                xmlSerializer = new XmlSerializer(typeof(TIn));

            return (type == SerializationType.XML ? (TIn)xmlSerializer.Deserialize(new StringReader(data.ToString())) : JsonConvert.DeserializeObject<TIn>(data.ToString()));
        }

        private IEnumerable<MPMessage> receive()
        {
            foreach (MPMessage msg in PyNet.getNewMessages(address, -1, -1))
                yield return (msg);
        }

    }

}

