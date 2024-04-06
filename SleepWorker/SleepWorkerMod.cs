using HarmonyLib;
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
        internal static IModHelper MHelper;
        internal static IMonitor MMonitor;
        public override void Entry(IModHelper helper)
        {
            MMonitor = Monitor;
            MHelper = helper;
            config = helper.ReadConfig<Config>();
            if (config.skiptime > 18 )
                config.skiptime = 18;

            if (config.skiptime < 1)
                config.skiptime = 1;

            helper.WriteConfig(config);

            var instance = new Harmony("Platonymous.SleepWorker");
            instance.Patch(AccessTools.Method(typeof(GameLocation), nameof(GameLocation.answerDialogueAction)), new HarmonyMethod(this.GetType(), nameof(AnswerDialoguePrefix)));


            helper.Events.GameLoop.GameLaunched += (s, e) => SetUpConfigMenu();
        }

        public static void AnswerDialoguePrefix(GameLocation __instance, ref string questionAndAnswer)
        {
            if (questionAndAnswer != "Sleep_Yes" || Game1.IsMultiplayer || canSleep)
                return;

            questionAndAnswer = "Sleep_No";
            Game1.playSound("coin");

            if (Game1.timeOfDay >= 2400)
                return;

            Task.Run(() =>
            {
                CcTime.TimeSkip(MHelper, Math.Min((config.skiptime * 100) + Game1.timeOfDay, 2400), () =>
                {
                    canSleep = true;
                    Game1.playSound("coin");
                    Game1.currentLocation.answerDialogueAction("Sleep_Yes", null);
                    canSleep = false;
                    Game1.hudMessages.Clear();
                }); ;
            });
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
