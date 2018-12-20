
using System.Collections.Generic;

using StardewValley;

namespace PelicanTTS
{
    class ModConfig
    {
        public float Pitch { get; set; }
        public float Volume { get; set; }
        public string Greeting { get; set; } = "Good Morning {Player}. It is {DayName} the {Day} day of {Season}.";
        public List<SpeechConfig> Voices { get; set; }

        public ModConfig()
        {
            List<SpeechConfig> villagers = new List<SpeechConfig>();
            Pitch = 0;
            Volume = 1;
            villagers.Add(new SpeechConfig("default", "Salli"));
            foreach (NPC npc in Utility.getAllCharacters())
            {
                if (npc.isVillager())
                {
                    int useAge = npc.age;
                    string voice = "Brian";
                    if (npc.gender == 1)
                        voice = npc.Age == 0 ? "Kendra" : "Salli";

                    switch (npc.name)
                    {
                        case "Elliot": voice = "Geraint"; break;
                        case "Alex": voice = "Joey"; break;
                        case "Sam": voice = "Russell"; break;
                        case "Emily": voice = "Emma"; break;
                        case "Haley": voice = "Emma"; break;
                        case "Harvey": voice = "Matthew"; break;
                        case "Wizard": voice = "Geraint"; break;
                        case "Pierre": voice = "Matthew"; break;
                        case "Morris": voice = "Geraint"; break;
                        case "Mister Qi": voice = "Geraint"; break;
                        case "Penny": voice = "Amy"; break;
                        case "Evelyn": voice = "Amy"; break;
                        case "Jas": voice = "Ivy"; break;
                        case "Jodi": voice = "Nicole"; break;
                        case "Marnie": voice = "Kimberly"; break;
                        case "Pam": voice = "Kimberly"; break;
                        case "Sandy": voice = "Raveena"; break;
                        case "Vincent": voice = "Justin"; break;
                        case "default": voice = "Salli"; break;
                        default: break;
                    }

                    SpeechConfig next = new SpeechConfig(npc.name, voice);
                    villagers.Add(next);
                }
            }

            if(!villagers.Exists(v => v.name == "Morris"))
                villagers.Add(new SpeechConfig("Morris", "Geraint"));

            Voices = villagers;
        }
    }
}
