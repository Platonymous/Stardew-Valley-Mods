
using System.Collections.Generic;

using StardewValley;

namespace PelicanTTS
{
    class ModConfig
    {
        public string lang { get; set; }
        public int rate { get; set; }
        public int volume { get; set; }
        public string polly { get; set; }
        public SpeechConfig[] voices { get; set; }

        public ModConfig()
        {
            List<SpeechConfig> villagers = new List<SpeechConfig>();
            lang = "en-us";
            polly = "off";
            rate = 0;
            volume = 100;
            villagers.Add(new SpeechConfig("default", "Microsoft Zira Desktop", 1, 0));
            foreach(NPC npc in Utility.getAllCharacters())
            {
                if (npc.isVillager())
                {
                    int useAge = npc.age;

                    if (npc.name == "Evelyn" || npc.name == "George" || npc.name == "Lewis")
                    {
                        useAge = 3;
                    }

                    SpeechConfig next = new SpeechConfig(npc.name, "any", npc.gender, useAge);
                    villagers.Add(next);
                }
            }
            voices = villagers.ToArray();
        }
    }
}
