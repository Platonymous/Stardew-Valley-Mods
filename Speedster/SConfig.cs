using Microsoft.Xna.Framework.Input;

namespace Speedster
{
    class SConfig
    {
        public Keys speedKey { get; set; }
        public Keys timeKey { get; set; }
        public int topSpeed { get; set; }
        public int normalSpeed { get; set; }

        public SConfig()
        {
            speedKey = Keys.Space;
            timeKey = Keys.OemComma;
            topSpeed = 24;
            normalSpeed = 6;
        }

    }
}
