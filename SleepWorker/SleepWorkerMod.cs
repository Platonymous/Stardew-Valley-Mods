using PyTK.ConsoleCommands;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Threading.Tasks;

namespace SleepWorker
{
    public class SleepWorkerMod : Mod
    {
        internal static Config config;
        internal static bool canSleep = false;
        const int maxSkip = 2400;
        public override void Entry(IModHelper helper)
        {
            config = helper.ReadConfig<Config>();
            if (config.skiptime > 18 )
                config.skiptime = 18;

            if (config.skiptime < 1)
                config.skiptime = 1;

            helper.WriteConfig(config);

            PyTK.Events.PyTimeEvents.BeforeSleepEvents += (s, e) =>
            {
                if(!Game1.IsMultiplayer && !canSleep)
                {
                    e.Response.responseKey = "No";
                    Game1.playSound("coin");

                    if (Game1.timeOfDay >= 2400)
                        return;

                    Task.Run(() =>
                    {
                        CcTime.TimeSkip(Math.Min((config.skiptime * 100) + Game1.timeOfDay,2400), () =>
                        {
                            canSleep = true;
                            Game1.playSound("coin");
                            Game1.currentLocation.lastQuestionKey = "Sleep";
                            Game1.currentLocation.answerDialogue(new Response("Yes", "Yes"));
                            canSleep = false;
                            Game1.hudMessages.Clear();
                        });;
                    });
                }
            };

            helper.Events.GameLoop.GameLaunched += (s, e) => SetUpConfigMenu();
        }

        public class Config
        {
            public int skiptime { get; set; } = 6;
        }

        private void SetUpConfigMenu()
        {
            if (!Helper.ModRegistry.IsLoaded("spacechase0.GenericModConfigMenu"))
                return;

            var api = Helper.ModRegistry.GetApi<IGMCMAPI>("spacechase0.GenericModConfigMenu");

            api.RegisterModConfig(ModManifest, () =>
            {
                config = new Config();
            }, () =>
            {
                Helper.WriteConfig<Config>(config);
            });

            api.RegisterLabel(ModManifest, ModManifest.Name, ModManifest.Description);
            api.RegisterClampedOption(ModManifest, "Max duration", "In hours", () => config.skiptime, (int b) => config.skiptime = Math.Max(1,Math.Min(b,18)),1,18);

        }

    }
}
