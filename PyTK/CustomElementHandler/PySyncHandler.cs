using PyTK.Types;
using StardewModdingAPI.Events;
using StardewValley;
using System;

namespace PyTK.CustomElementHandler
{
    public class PySyncHandler<T> : IPyResponder where T : class
    {
        private PyReceiver<T> receiver;
        private string receiverName;
        private int interval;
        private Action<T> syncReceiver;
        private Func<T> syncSender;
        public string address { get; set; }

        public PySyncHandler(string uniqueId, int interval, Action<T> syncReceiver, Func<T> syncSender)
        {
            this.syncSender = syncSender;
            this.syncReceiver = syncReceiver;
            this.interval = interval;
            receiverName = "PYTK.Sync." + uniqueId;
            address = receiverName;
            receiver = new PyReceiver<T>(receiverName, syncReceiver, interval, SerializationType.JSON);
        }

        public void start()
        {
            receiver.start();
            PyTKMod._events.GameLoop.UpdateTicked += runSync;
        }

        public void stop()
        {
            receiver.stop();
            PyTKMod._events.GameLoop.UpdateTicked -= runSync;
        }

        private void runSync(object sender, UpdateTickedEventArgs e)
        {
            if (!Game1.IsMultiplayer)
                return;

            if (!e.IsMultipleOf((uint)interval))
                return;

            T data = syncSender();

            if (data != null)
                    PyNet.sendDataToFarmer(receiverName, data, -1, SerializationType.JSON);
        }
    }
}
