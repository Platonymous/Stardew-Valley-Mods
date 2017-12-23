using Microsoft.Xna.Framework.Input;

namespace Visualize
{
    internal class Config
    {
        public string activeProfile { get; set; } = "Platonymous.Original";
        public float saturation { get; set; } = 100;
        public Keys next { get; set; } = Keys.PageDown;
        public Keys previous { get; set; } = Keys.PageUp;
        public Keys satHigher { get; set; } = Keys.NumPad9;
        public Keys satLower { get; set; } = Keys.NumPad6;
        public Keys reset { get; set; } = Keys.NumPad0;
        public int passes { get; set; } = 10;

    }
}
