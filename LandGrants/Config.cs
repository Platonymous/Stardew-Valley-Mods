using StardewModdingAPI;

namespace LandGrants
{
    public class Config
    {
        public bool KeepFarmsActive { get; set; } = false;

        public int MaxPlayer { get; set; } = 16;

        public SButton BuildCabinKey { get; set; } = SButton.F10;
    }
}
