using StardewModdingAPI;

namespace Speedster
{
    class SConfig
    {
        public SButton speedKey { get; set; }
        public SButton timeKey { get; set; }
        public int topSpeed { get; set; }
        public int normalSpeed { get; set; }

        public SConfig()
        {
            speedKey = SButton.Space;
            timeKey = SButton.OemComma;
            topSpeed = 24;
            normalSpeed = 6;
        }

    }
}
