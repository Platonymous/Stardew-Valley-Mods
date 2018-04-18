using PyTK.ConsoleCommands;
using StardewModdingAPI;
using StardewValley;


namespace SleepWorker
{
    public class SleepWorkerMod : Mod
    {
        public override void Entry(IModHelper helper)
        {
            Config config = helper.ReadConfig<Config>();
            PyTK.Events.PyTimeEvents.OnSleepEvents += (s, e) => { if (Game1.timeOfDay < config.maxskip) CcTime.TimeSkip(Game1.timeOfDay < (config.maxskip - config.skiptime) ? (Game1.timeOfDay + config.skiptime).ToString() : config.maxskip.ToString(), false); };
        }

        public class Config
        {
            public int maxskip { get; set; } = 1950;
            public int skiptime { get; set; } = 500;
        }
    }
}
