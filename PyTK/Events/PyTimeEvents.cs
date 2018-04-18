using System;
using PyTK.Overrides;
using PyTK.Types;

namespace PyTK.Events
{
    public static class PyTimeEvents
    {

        public static event EventHandler<EventArgs> OnSleepEvents;

        public class EventArgsSleep : EventArgs
        {
            public EventArgsSleep(STime sleepTime, bool passedOut)
            {
                SleepTime = sleepTime;
                PassedOut = passedOut;
            }

            public bool PassedOut { get; }
            public STime SleepTime { get; }
        }


        internal static void CallOnSleepEvents(object sender, EventArgsSleep e)
        {
            OnSleepEvents?.Invoke(sender, e);
        }

    }
}
