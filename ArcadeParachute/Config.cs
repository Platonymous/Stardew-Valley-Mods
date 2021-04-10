using StardewModdingAPI;

namespace ArcadeParachute
{
    class Config
    {
        public SButton debugKey { get; set; }

        public Config()
        {
            debugKey = SButton.J;
        }
    }
}
