using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;

namespace GeodeCrusherExtension
{
    public class Config
    {
        public string MachineID { get; set; } = "[CFR] Geode Crusher.GeodeCrusher.json.0";
    }

    public interface IHandlerAPI
    {
        void setOutputHandler(string machineId, Func<StardewValley.Object, StardewValley.Object, string, string, StardewValley.Object> outputHandler);
    }

    public class GeodeCrusherExtensionMod : Mod
    {
        private Config config;

        public override void Entry(IModHelper helper)
        {
            config = helper.ReadConfig<Config>();
            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;

        }
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            IHandlerAPI api = this.Helper.ModRegistry.GetApi<IHandlerAPI>("Platonymous.CustomFarming");

            api.setOutputHandler(config.MachineID, (obj, o, m, r) =>
            {
                ++Game1.stats.GeodesCracked;
                var item = Utility.getTreasureFromGeode(obj.getOne());
                if (item.Type.Contains("Mineral"))
                    Game1.player.foundMineral(item.ParentSheetIndex);
                else if (item.Type.Contains("Arch") && !Game1.player.hasOrWillReceiveMail("artifactFound"))
                    item = new StardewValley.Object(390, 5, false, -1, 0);

                return item;
            });
        }

    }
}
