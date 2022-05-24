using StardewModdingAPI;

namespace GhostTown
{
    public class GhostTownMod : Mod
    {
        internal static Config config;

        public override void Entry(IModHelper helper)
        {
            config = helper.ReadConfig<Config>();
            helper.Events.Content.AssetRequested += new Ghostify(helper).OnAssetRequested;
        }
    }
}
