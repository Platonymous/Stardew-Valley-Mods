using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Newtonsoft.Json;
using StardewModdingAPI.Events;
using System.Threading.Tasks;
using System.Linq;

namespace PyTK.Types
{

    public class PyReceiver<TIn> : IPyResponder
    {
        public string address { get; set; }
        public int interval;
        public XmlSerializer xmlSerializer;
        public Action<TIn> requestHandler;
        public SerializationType serializationType;
        public SerializationType requestSerialization;
        private int tick;

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
            tick = 0;
            PyTKMod._events.GameLoop.UpdateTicked += checkForRequests;
        }

        public void stop()
        {
            PyTKMod._events.GameLoop.UpdateTicked -= checkForRequests;
        }

        private void checkForRequests(object sender, UpdateTickedEventArgs e)
        {
            tick++;
            if (tick != interval)
                return;

            List<MPMessage> messages = receive().ToList();

            if (messages.Count > 0)
                foreach (MPMessage request in messages)
                    Task.Run(() => { requestHandler(deserialize(requestSerialization, request.message)); ; });

            tick = 0;
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

