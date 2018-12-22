namespace PelicanTTS
{
    class ModConfig
    {
        public float Pitch { get; set; }
        public float Volume { get; set; }
        public bool MumbleDialogues { get; set; }

        public ModConfig()
        {
            Pitch = 0;
            Volume = 1;
            MumbleDialogues = false;
        }
    }
}
