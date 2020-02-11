using StardewModdingAPI;

namespace PlatoWarpMenu
{
    class Config
    {
        public SButton MenuButton { get; set; }

        public Config()
        {
            MenuButton = SButton.J;
        }
    }
}
