using StardewModdingAPI;

namespace HarmonyMoviesTest
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
