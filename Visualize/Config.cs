using StardewModdingAPI;

namespace Visualize
{
    internal class Config
    {
        public string activeProfile { get; set; } = "Platonymous.Original";
        public float saturation { get; set; } = 100;
        public SButton next { get; set; } = SButton.PageDown;
        public SButton previous { get; set; } = SButton.PageUp;
        public SButton satHigher { get; set; } = SButton.NumPad9;
        public SButton satLower { get; set; } = SButton.NumPad6;
        public SButton reset { get; set; } = SButton.NumPad0;
        public int passes { get; set; } = 10;

    }
}
