using System;
using PyTK.Overrides;
using PyTK.Types;
using StardewValley;

namespace PyTK.Events
{
    public static class PyTimeEvents
    {
        public static event EventHandler<EventArgsBeforeSleep> BeforeSleepEvents;

        public class EventArgsBeforeSleep : EventArgs
        {
            public EventArgsBeforeSleep(STime sleepTime, bool passedOut, ref Response response)
            {
                SleepTime = sleepTime;
                PassedOut = passedOut;
                Response = response;
            }

            public bool PassedOut { get; }
            public STime SleepTime { get; }
            public Response Response { get; set; }

        }

        internal static void CallBeforeSleepEvents(object sender, EventArgsBeforeSleep e)
        {
            BeforeSleepEvents?.Invoke(sender, e);
        }

    }
}
