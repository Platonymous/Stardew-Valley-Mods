using StardewModdingAPI;
using PyTK.CustomElementHandler;
using System.Collections.Generic;

namespace GhostTown
{
    public class GhostTownMod : Mod
    {
        internal static Config config;
        internal List<CustomObjectData> customObjects;

        public override void Entry(IModHelper helper)
        {
            config = helper.ReadConfig<Config>();
            helper.Content.AssetEditors.Add(new Ghostify(helper));
        }
    }
}
