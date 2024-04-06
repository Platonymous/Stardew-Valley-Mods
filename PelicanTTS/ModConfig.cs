﻿using StardewModdingAPI;
using System.Collections.Generic;

namespace PelicanTTS
{
    public class ModConfig
    {
        public float Pitch { get; set; } = 0;
        public float Volume { get; set; } = 1;
        public bool MumbleDialogues { get; set; } = false;
        public bool Greeting { get; set; } = true;

        public bool ReadDialogues { get; set; } = true;

        public bool ReadSelectedDialogueResponse { get; set; } = true;

        public bool ReadNonCharacterMessages{ get; set; } = true;

        public bool ReadLetters { get; set; } = true;

        public bool ReadHudMessages { get; set; } = true;

       // public bool ReadChatMessages { get; set; } = true;

        public int Rate { get; set; } = 100;

        public string LanguageCode { get; set; } = "";

        public SButton ReadScreenKey { get; set; } = SButton.N;

        public Dictionary<string, VoiceSetup> Voices { get; set; } = new Dictionary<string, VoiceSetup>();

        public bool UseNeuralVoices { get; set; } = true;

        public string Server { get; set; } = "USA";
    }
}
