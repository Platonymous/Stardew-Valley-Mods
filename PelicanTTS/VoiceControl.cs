using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Speech.Synthesis;
using System.Speech.Recognition;
using System.Globalization;
using StardewModdingAPI;
using StardewValley;
using Microsoft.Xna.Framework.Input;

namespace PelicanTTS
{
 /*   
    class VoiceControl
    {
        private static SpeechRecognitionEngine rec;
        private static IMonitor Monitor;
        private static string recText;
        private static string last;

        public VoiceControl()
        {
          
        }

        public static void start(IMonitor m)
        {
            Monitor = m;
            rec = new SpeechRecognitionEngine(CultureInfo.CreateSpecificCulture("en-US"));
            setupGrammar();
            rec.SpeechRecognized += Rec_SpeechRecognized;
            rec.SetInputToDefaultAudioDevice();
            rec.RecognizeAsync(RecognizeMode.Multiple);
            recText = "";
            last = "";

        }

        public static void stop()
        {
            rec.RecognizeAsyncCancel();
        }

        private static void setupGrammar()
        {
            Choices choices = new Choices();

            choices.Add("switch");
            choices.Add("next");
            choices.Add("back");
            choices.Add("hit");
            choices.Add("axe");
            choices.Add("can");
            choices.Add("pickaxe");
            choices.Add("harp");
            choices.Add("hoe");
            choices.Add("get");
            choices.Add("talk");

            foreach (NPC npc in Utility.getAllCharacters())
            {
                if (npc.isVillager())
                {
                    choices.Add("hey "+npc.name.ToLower());
                }
            }

            GrammarBuilder builder = new GrammarBuilder(choices);
            builder.Culture = CultureInfo.CreateSpecificCulture("en-US");
            Grammar wordlist = new Grammar(builder);
            rec.LoadGrammar(wordlist);
            //rec.LoadGrammar(new DictationGrammar());
        
        }

        private static void Rec_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
           
                recText = e.Result.Text;
            
            
            
            Monitor.Log(e.Result.Text + ":" + recText);

            if(recText == "walk")
            {
             
            }

            if (last == "get")
            {

                if (recText == "switch")
                {
                    Game1.player.shiftToolbar(true);
                }

                if (recText == "hit")
                {
                    Game1.pressUseToolButton();
                    Game1.releaseUseToolButton();
                }

                if (recText == "next")
                {
                    Game1.player.CurrentToolIndex = (Game1.player.CurrentToolIndex + 1) % 12;
                }

                if (recText == "back")
                {
                    Game1.player.CurrentToolIndex = (Game1.player.CurrentToolIndex - 1) % 12;
                    if (Game1.player.CurrentToolIndex < 0)
                    {
                        Game1.player.CurrentToolIndex = 11;
                    }

                }

                if (recText == "hoe")
                {
                    int index = Game1.player.items.FindIndex(x => x.Name.ToLower().Contains("hoe"));
                    if (index >= 0)
                    {
                        Game1.player.CurrentToolIndex = index;
                    }
                }

                if (recText == "can")
                {
                    int index = Game1.player.items.FindIndex(x => x.Name.ToLower().Contains("watering"));
                    if (index >= 0)
                    {
                        Game1.player.CurrentToolIndex = index;
                    }
                }

                if (recText == "pickaxe")
                {
                    int index = Game1.player.items.FindIndex(x => x.Name.ToLower().Contains("pickaxe"));
                    if (index >= 0)
                    {
                        Game1.player.CurrentToolIndex = index;
                    }
                }

                if (recText == "axe")
                {
                    int index = Game1.player.items.FindIndex(x => x.Name.ToLower().Contains("axe") && !x.Name.ToLower().Contains("pickaxe"));
                    if (index >= 0)
                    {
                        Game1.player.CurrentToolIndex = index;
                    }
                }

                if (recText == "harp")
                {
                    int index = Game1.player.items.FindIndex(x => x.Name.ToLower().Contains("harp of yoba"));
                    if (index >= 0)
                    {
                        Game1.player.CurrentToolIndex = index;
                    }
                }
            }

            if(last == "talk")
            {
           
                foreach( NPC npc in Game1.currentLocation.characters)
                {
                    if ("hey "+npc.name.ToLower() == recText)
                    {
                        npc.showTextAboveHead("Hey " + Game1.player.name);
                    PelicanTTSMod.say("Hey " + Game1.player.name);
                        PelicanTTSMod.say("");
                    }
                }
            }

            last = recText;
            recText = "";
            

            
                
        }

      
    }*/
}
