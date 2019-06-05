using StardewModdingAPI;

namespace Portraiture
{
    class PConfig
    {
        public SButton changeKey { get; set; } = SButton.P;
        public SButton menuKey { get; set; } = SButton.M;
        public string active { get; set; } = "none";
    }
}
