/*

using System.Speech.Synthesis;
using System.Globalization;
using System.Collections.Generic;
using System.Threading;

using StardewModdingAPI;
using StardewModdingAPI.Events;

using StardewValley;
using StardewValley.Menus;


namespace PelicanTTS
{
    internal class SpeechHandler
    {
        private static SpeechSynthesizer synth;
        private static string lastText;
        private static string lastDialog;
        private static string lastHud;
        public static string currentText;
        private static Thread speechThread;
        private static bool runSpeech;
        private static IModHelper Helper;
        private static IMonitor Monitor;
        private static CultureInfo currentCulture;
        public static string culturelang;
        private static List<string> installedVoices;


        private static Dictionary<string, SpeechProfile> voices;

        public SpeechHandler()
        {

        }

        public static void start(IModHelper h, IMonitor m)
        {
            Monitor = m;
            Helper = h;
            synth = new SpeechSynthesizer();
            synth.SetOutputToDefaultAudioDevice();
            lastText = "";
            currentText = "";
            lastDialog = "";
            lastHud = "";
            runSpeech = true;
            currentCulture = CultureInfo.CreateSpecificCulture("en-us");
            culturelang = "en";
            installedVoices = new List<string>();
            foreach (InstalledVoice voice in synth.GetInstalledVoices())
            {
                string info = voice.VoiceInfo.Name;
                installedVoices.Add(info);
            }

            setupVoices();
            setVoice("default");

            speechThread = new Thread(t2sOut);
            speechThread.Start();
            h.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            h.Events.Display.MenuChanged += OnMenuChanged;
           
        }


        private static void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (e.NewMenu == null)
            {
                lastText = "";
                lastDialog = "";
                currentText = "";
                synth.SpeakAsyncCancelAll();
                setVoice("default");
            }
        }

        public static void stop()
        {
            Helper.Events.GameLoop.UpdateTicked -= OnUpdateTicked;
            Helper.Events.Display.MenuChanged -= OnMenuChanged;
            runSpeech = false;
        }




        public static void setVoice(string name)
        {
   
            if (!voices.ContainsKey(name)) { name = "default"; }

            if (installedVoices.Contains(voices[name].name))
            {
                synth.SelectVoice(voices[name].name);
            }
            else
            {
                synth.SelectVoiceByHints(voices[name].gender, voices[name].age, voices[name].alternate, voices[name].culture);
            }

           
        }


        public static void setupVoices()
        {
            ModConfig config = Helper.ReadConfig<ModConfig>();
            currentCulture = CultureInfo.CreateSpecificCulture(config.lang);
            culturelang = config.lang.Split('-')[0];
            synth.Rate = config.rate;
            synth.Volume = config.volume;
            voices = new Dictionary<string, SpeechProfile>();

            foreach (SpeechConfig npc in config.voices)
            {
                VoiceGender gender = (npc.gender == 0) ? VoiceGender.Male : VoiceGender.Female;

                VoiceAge age = VoiceAge.NotSet;

                if (npc.age == 3)
                {
                    age = VoiceAge.Senior;
                }
                else if (npc.age == 0)
                {
                    age = VoiceAge.Adult;
                }
                else if(npc.age == 1)
                {
                    age = VoiceAge.Teen;
                }else if(npc.age == 2)
                {
                    age = VoiceAge.Child;
                }

                voices.Add(npc.name, new SpeechProfile(npc.voicename, gender, age, 0, currentCulture));
            }

        }

        public static bool HasMethod(object objectToCheck, string methodName)
        {
            var type = objectToCheck.GetType();
            return type.GetMethod(methodName) != null;
        }


        private static void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (e.IsMultipleOf(15)) // quarter second
            {
                if (Game1.activeClickableMenu is DialogueBox)
                {

                    DialogueBox dialogueBox = (DialogueBox)Game1.activeClickableMenu;

                    if (dialogueBox.isPortraitBox() && Game1.currentSpeaker != null)
                    {
                        setVoice(Game1.currentSpeaker.name);
                    }
                    else
                    {
                        setVoice("default");
                    }

                    if (dialogueBox.getCurrentString() != lastDialog)
                    {
                        currentText = dialogueBox.getCurrentString();
                        lastDialog = dialogueBox.getCurrentString();

                    }

                }
                else if (Game1.activeClickableMenu is LetterViewerMenu lvm)
                {
                    setVoice("default");
                    List<string> mailMessage = Helper.Reflection.GetField<List<string>>(lvm, "mailMessage").GetValue();
                    string letter = mailMessage[Helper.Reflection.GetField<int>(lvm, "page").GetValue()];
                    currentText = letter;
                    lastDialog = letter;
                }
                else if(Game1.hudMessages.Count > 0)
                {
                    if(Game1.hudMessages[Game1.hudMessages.Count-1].Message != lastHud)
                    {
                        setVoice("default");
                        currentText = Game1.hudMessages[Game1.hudMessages.Count - 1].Message;
                        lastHud = Game1.hudMessages[Game1.hudMessages.Count - 1].Message;
                    }
                }
            }
        }

        private static void t2sOut()
        {
            while (runSpeech)
            {
                if (currentText == lastText) { continue; }


                if(currentText.StartsWith("+")){
                    continue;
                }

                synth.SpeakAsyncCancelAll();

                synth.SpeakAsync(currentText);

                lastText = currentText;
            }
        }

        public static void demoVoices()
        {
            foreach (string voice in installedVoices)
            {
                synth.SelectVoice(voice);
                synth.Speak(voice);
            }
        }

        public static void showInstalledVoices()
        {
            Game1.activeClickableMenu = null;

            string text = "Installed Voices:^" + string.Join(", ", installedVoices);

            Game1.activeClickableMenu = new DialogueBox(text);
        }

    }
}
*/