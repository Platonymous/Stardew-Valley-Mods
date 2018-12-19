using StardewModdingAPI;

namespace GhostTown
{
    public class GhostTownMod : Mod
    {
        internal static Config config;

        public override void Entry(IModHelper helper)
        {
            config = helper.ReadConfig<Config>();
            helper.Content.AssetEditors.Add(new Ghostify(helper));
        }
    }
}
