using StardewModdingAPI;
using System.Collections.Generic;

namespace Portraiture
{
    class PConfig
    {
        public SButton changeKey { get; set; } = SButton.P;
        public SButton menuKey { get; set; } = SButton.M;
        public SButton fixPortraitKey { get; set; } = SButton.O;

        public SButton styleChangeKey { get; set; } = SButton.I;

        public bool ShowPortraitsAboveBox { get; set; } = false;

        public int MaxAbovePortraitPercent { get; set; } = 80;

        public bool SideLoadHDPWhenNotInstalled { get; set; } = false;

        public bool SideLoadHDPWhenInstalled { get; set; } = false;

        public bool HPDOption { get; set; } = true;
        public string active { get; set; } = "none";

        public PresetCollection presets { get; set; } = new PresetCollection();

    }

    public class PresetCollection
    {
        public List<Preset> Presets { get; set; } = new List<Preset>();
    }

    public class Preset
    {
        public string Character { get; set; } = "";
        public string Portraits { get; set; } = "";
    }
}
