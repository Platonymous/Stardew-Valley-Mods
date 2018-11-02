using PyTK.ConsoleCommands;
using StardewModdingAPI;
using StardewValley;
using System.Threading;

namespace SleepWorker
{
    public class SleepWorkerMod : Mod
    {
        internal static Config config;
        internal static bool canSleep = false;
        public override void Entry(IModHelper helper)
        {
            config = helper.ReadConfig<Config>();
            PyTK.Events.PyTimeEvents.BeforeSleepEvents += (s, e) =>
            {
                if (!Game1.IsMultiplayer && !canSleep)
                {
                    e.Response.responseKey = "No"; Thread tThread = new Thread(callTimeSkip); tThread.Start();
                }
            };
        }
        public void callTimeSkip()
        {
            Game1.playSound("coin");

            if (Game1.timeOfDay < config.maxskip)
                CcTime.TimeSkip(Game1.timeOfDay < (config.maxskip - config.skiptime) ? (Game1.timeOfDay + config.skiptime).ToString() : config.maxskip.ToString(), false);

            Game1.playSound("coin");
            canSleep = true;
            Game1.currentLocation.lastQuestionKey = "Sleep";
            Game1.currentLocation.answerDialogue(new Response("Yes", "Yes"));
            canSleep = false;
        }

        public class Config
        {
            public int maxskip { get; set; } = 1950;
            public int skiptime { get; set; } = 1200;
        }

    }
}
