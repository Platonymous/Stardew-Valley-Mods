
using System.Collections.Generic;

using StardewValley;

namespace PelicanTTS
{
    class ModConfig
    {
        public float Pitch { get; set; }
        public float Volume { get; set; }

        public ModConfig()
        {
            Pitch = 0;
            Volume = 1;
        }
    }
}
