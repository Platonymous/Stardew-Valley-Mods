using System.Collections.Generic;

namespace PelicanTTS
{
    public class ModConfig
    {
        public float Pitch { get; set; } = 0;
        public float Volume { get; set; } = 1;
        public bool MumbleDialogues { get; set; } = false;
        public bool Greeting { get; set; } = true;

        public Dictionary<string, VoiceSetup> Voices { get; set; } = new Dictionary<string, VoiceSetup>();

    }
}
