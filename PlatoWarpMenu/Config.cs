using StardewModdingAPI;

namespace PlatoWarpMenu
{
    public class Config
    {
        public SButton MenuButton { get; set; }

        public bool UseTempFolder { get; set; }

        public Config()
        {
            MenuButton = SButton.J;
            UseTempFolder = false;
        }
    }
}
