using System.Speech.Synthesis;
using System.Globalization;
using System.Collections.Generic;
using System.Threading;

using StardewModdingAPI;
using StardewModdingAPI.Events;

using StardewValley;
using StardewValley.Menus;



using Amazon.Polly;
using Amazon.Polly.Model;

using Microsoft.Xna.Framework.Media;
using System.IO;

using System;

namespace PelicanTTS
{
    internal class SpeechHandlerPolly
    {
        private static string lastText;
        private static string lastDialog;
        private static string lastHud;
        public static string currentText;
        private static Thread speechThread;
        private static Thread gThread;
        private static bool runSpeech;
        private static IModHelper Helper;
        private static CultureInfo currentCulture;
        public static string culturelang;
        private static List<string> installedVoices;


        private static AmazonPollyClient pc;
        private static VoiceId currentVoice;
        private static string tmppath;
        public static bool speak;
        private static string speakerName;

        public static IMonitor Monitor;

        public SpeechHandlerPolly()
        {

        }

        public static void start(IModHelper h)
        {
           
            Helper = h;
            currentText = "";
            tmppath = Path.Combine(Path.Combine(Environment.CurrentDirectory, "Content"), "TTS");

            ensureFolderStructureExists(Path.Combine(tmppath, "speech.mp3"));
            pc = AWSHandler.getPollyClient();
            currentVoice = VoiceId.Amy;
            lastText = "";
            lastDialog = "";
            lastHud = "";
            speak = false;
            runSpeech = true;
            currentCulture = CultureInfo.CreateSpecificCulture("en-us");
            culturelang = "en";
            installedVoices = new List<string>();


            setupVoices();
            setVoice("default");

            speechThread = new Thread(t2sOut);
            speechThread.Start();
            GameEvents.QuarterSecondTick += GameEvents_QuarterSecondTick;

            MenuEvents.MenuClosed += MenuEvents_MenuClosed;

            //ControlEvents.KeyPressed += ControlEvents_KeyPressed;

        }

        private static void ControlEvents_KeyPressed(object sender, EventArgsKeyPressed e)
        {
            
            if (e.KeyPressed.ToString() == "K")
            {
                gThread = new Thread(generateAllDialogs);
                gThread.Start();
            }
            

        }

        private static void MenuEvents_MenuClosed(object sender, EventArgsClickableMenuClosed e)
        {
            lastText = "";
            lastDialog = "";
            currentText = "";
            MediaPlayer.Stop();
            setVoice("default");
        }

        public static void stop()
        {
            GameEvents.QuarterSecondTick -= GameEvents_QuarterSecondTick;
            MenuEvents.MenuClosed -= MenuEvents_MenuClosed;
            runSpeech = false;
        }




        public static void setVoice(string name)
        {
            speakerName = name;
            NPC speaker = Game1.getCharacterFromName(name);

            if (speaker != null && speaker.gender == 0)
            {
                if (speaker.age == 0)
                {
                    currentVoice = VoiceId.Joey;
                }
                else
                {
                    currentVoice = VoiceId.Joey;
                }

            }


            if (speaker != null && speaker.gender == 1)
            {
                if (speaker.age == 0)
                {
                    currentVoice = VoiceId.Kendra;
                }
                else
                {
                    currentVoice = VoiceId.Salli;
                }

            }


            switch (name)
            {
                case "Elliot": currentVoice = VoiceId.Geraint; break;
                case "Sam": currentVoice = VoiceId.Russell; break;
                case "Emily": currentVoice = VoiceId.Emma; break;
                case "Haley": currentVoice = VoiceId.Emma; break;
                case "Harvey": currentVoice = VoiceId.Matthew; break;
                case "George": currentVoice = VoiceId.Brian; break;
                case "Linus": currentVoice = VoiceId.Brian; break;
                case "Lewis": currentVoice = VoiceId.Brian; break;
                case "Governor": currentVoice = VoiceId.Brian; break;
                case "Grandpa": currentVoice = VoiceId.Brian; break;
                case "Clint": currentVoice = VoiceId.Brian; break;
                case "Willy": currentVoice = VoiceId.Brian; break;
                case "Wizard": currentVoice = VoiceId.Geraint; break;
                case "Pierre": currentVoice = VoiceId.Matthew; break;
                case "Gunther": currentVoice = VoiceId.Brian; break;
                case "Govenor": currentVoice = VoiceId.Brian; break;
                case "Marlon": currentVoice = VoiceId.Brian; break;
                case "Morris": currentVoice = VoiceId.Geraint; break;
                case "Mister Qi": currentVoice = VoiceId.Geraint; break;
                case "Gil": currentVoice = VoiceId.Brian; break;
                case "Penny": currentVoice = VoiceId.Amy; break;
                case "Evelyn": currentVoice = VoiceId.Amy; break;
                case "Jas": currentVoice = VoiceId.Ivy; break;
                case "Jodi": currentVoice = VoiceId.Nicole; break;
                case "Marnie": currentVoice = VoiceId.Kimberly; break;
                case "Pam": currentVoice = VoiceId.Kimberly; break;
                case "Sandy": currentVoice = VoiceId.Raveena; break;
                case "Vincent": currentVoice = VoiceId.Justin; break;
                case "default": currentVoice = VoiceId.Salli; break;


                default: break;
            }


        }


        public static void setupVoices()
        {
            ModConfig config = Helper.ReadConfig<ModConfig>();
            
            currentCulture = CultureInfo.CreateSpecificCulture(config.lang);
            culturelang = config.lang.Split('-')[0];


        }

        public static bool HasMethod(object objectToCheck, string methodName)
        {
            var type = objectToCheck.GetType();
            return type.GetMethod(methodName) != null;
        }


        private static void GameEvents_QuarterSecondTick(object sender, System.EventArgs e)
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
            else if (Game1.hudMessages.Count > 0)
            {
                if (Game1.hudMessages[Game1.hudMessages.Count - 1].Message != lastHud)
                {
                    setVoice("default");
                    currentText = Game1.hudMessages[Game1.hudMessages.Count - 1].Message;
                    lastHud = Game1.hudMessages[Game1.hudMessages.Count - 1].Message;
                }
            }

        }


        private static FileInfo ensureFolderStructureExists(string path)
        {

            FileInfo fileInfo1 = new FileInfo(path);

            if (!fileInfo1.Directory.Exists)
                fileInfo1.Directory.Create();

            return fileInfo1;
        }


        private static void t2sOut()
        {
            while (runSpeech )
            {
                if (currentText == lastText) { continue; }


                if (currentText.StartsWith("+"))
                {
                    continue;
                }

              
                currentText = currentText.Replace('^', ' ').Replace(Environment.NewLine, " ").Replace("$s", "").Replace("$h", "").Replace("$g", "").Replace("$e", "").Replace("$u", "").Replace("$b", "").Replace("$8", "").Replace("$l", "").Replace("$q", "").Replace("$9", "").Replace("$a", "").Replace("$7", "").Replace("<", "").Replace("$r", "");
                if (Game1.player.isMarried())
                {
                    currentText = currentText.Replace(" " + Game1.player.spouse + " ", " your spouse ").Replace(" " + Game1.player.spouse, " your spouse").Replace(Game1.player.spouse + " ", "Your spouse ");
                }
                currentText.Replace(" " + Game1.player.name + " ", " Farmer ").Replace(" " + Game1.player.name, " Farmer").Replace(Game1.player.name + " ", "Farmer ");
                
                int hash = currentText.GetHashCode();
                ensureFolderStructureExists(Path.Combine(Path.Combine(tmppath, speakerName), "speech.mp3"));
                string file = Path.Combine(Path.Combine(tmppath, speakerName), "speech" + hash + ".mp3");
                FileInfo fileInfo = new FileInfo(file);

                if (!fileInfo.Exists)
                {

                    SynthesizeSpeechRequest sreq = new SynthesizeSpeechRequest();
                    sreq.Text = currentText;
                    sreq.OutputFormat = OutputFormat.Mp3;
                    sreq.VoiceId = currentVoice;
                    SynthesizeSpeechResponse sres = pc.SynthesizeSpeech(sreq);

                    using (var fileStream = File.Create(file))
                    {
                        sres.AudioStream.CopyTo(fileStream);
                        fileStream.Flush();
                        fileStream.Close();
                    }
                }
                MediaPlayer.Stop();
                if (Game1.activeClickableMenu is DialogueBox || Game1.hudMessages.Count > 0 || speak)
                {
                    speak = false;
                    MediaPlayer.Play(Song.FromUri("speech" + hash, new System.Uri(Path.Combine(Path.Combine(Path.Combine("Content", "TTS"), speakerName), "speech" + hash + ".mp3"), System.UriKind.Relative)));
                }
                lastText = currentText;
            }
        }

        public static void demoVoices()
        {
        }

        public static void showInstalledVoices()
        {

        }

        
        public static void generateAllDialogs()
        {
            Monitor.Log("Generating Dialog");

                Dictionary<string, string> data = new Dictionary<string, string>();
                data = Game1.content.Load<Dictionary<string, string>>("Data\\TV\\TipChannel");
                
                List<string> dialogs = new List<string>(data.Values);
                Monitor.Log(dialogs.Count + " Entries found. ");
                setVoice("default");

                foreach (string text in dialogs)
                {
                Monitor.Log("next:"+text);
                string nextText = text;
            
                            nextText = nextText.Replace('^', ' ').Replace(Environment.NewLine, " ").Replace("$s", "").Replace("$h", "").Replace("$g", "").Replace("$e", "").Replace("$u", "").Replace("$b", "").Replace("$8", "").Replace("$l", "").Replace("$q", "").Replace("$9", "").Replace("$a", "").Replace("$7", "").Replace("<", "").Replace("$r", "");

                           
                            int hash = nextText.GetHashCode();
                    
                            string file = Path.Combine(Path.Combine(tmppath, speakerName), "speech" + hash + ".mp3");
                            FileInfo fileInfo = new FileInfo(file);


                            if (!fileInfo.Exists)
                            {

                                SynthesizeSpeechRequest sreq = new SynthesizeSpeechRequest();
                                sreq.Text = nextText;
                                sreq.OutputFormat = OutputFormat.Mp3;
                                sreq.VoiceId = currentVoice;
                                SynthesizeSpeechResponse sres = pc.SynthesizeSpeech(sreq);

                                using (var fileStream = File.Create(file))
                                {
                                    sres.AudioStream.CopyTo(fileStream);
                                    fileStream.Flush();
                                    fileStream.Close();
                                }
                            }
                            Monitor.Log(nextText);
                           
                        
                    
                }

            foreach (NPC npc in Utility.getAllCharacters())
            {

                /*for (int sp = 0; sp < 2; sp++) {
                    string name = npc.name;
                    string plus = "";
                    Dictionary<string, string> data = new Dictionary<string, string>();
                    if (sp == 1)
                    {
                        plus = "MarriageDialogue";
                    }
                    try
                    {
                        data = Game1.content.Load<Dictionary<string, string>>("Characters\\Dialogue\\"+ plus + name);
                    }
                    catch
                    {
                        continue;
                    }
                    List<string> dialogs = new List<string>(data.Values);
                    Monitor.Log(dialogs.Count + " Entries found for " + name);
                    setVoice(name);
                    Game1.player.name = "Farmer";

                    for (int s = 0; s < 4; s++)
                    {
                        if (s == 0)
                        {
                            Game1.currentSeason = "spring";
                        }

                        if (s == 1)
                        {
                            Game1.currentSeason = "summer";
                        }

                        if (s == 2)
                        {
                            Game1.currentSeason = "fall";
                        }

                        if (s == 3)
                        {
                            Game1.currentSeason = "winter";
                        }

                        for (int day = 1; day < 1; day++)
                        {
                            Game1.dayOfMonth = day;
                            try
                            {

                                foreach (string text in dialogs)
                                {
                                    try { 
                                    Dialogue d = new Dialogue(text, Game1.getCharacterFromName(name));
                                    DialogueBox db = new DialogueBox(d);
                                    List<string> dl = Helper.Reflection.GetPrivateValue<List<string>>(d, "dialogues");
                                    Monitor.Log("Dialog Length:" + dl.Count);

                                    while (dl.Count > 0)
                                    {
                                        db = new DialogueBox(d);
                                        string nextText = db.getCurrentString();
                                        nextText = nextText.Replace('^', ' ').Replace(Environment.NewLine, " ").Replace("$s", "").Replace("$h", "").Replace("$g", "").Replace("$e", "").Replace("$u", "").Replace("$b", "").Replace("$8", "").Replace("$l", "").Replace("$q", "").Replace("$9", "").Replace("$a", "").Replace("$7", "").Replace("<", "").Replace("$r", "");

                                        if (Game1.player.isMarried())
                                        {
                                            nextText = nextText.Replace(" " + Game1.player.spouse + " ", " your spouse ").Replace(" " + Game1.player.spouse, " your spouse").Replace(Game1.player.spouse + " ", "Your spouse ");
                                        }
                                        nextText.Replace(" " + Game1.player.name + " ", " Farmer ").Replace(" " + Game1.player.name, " Farmer").Replace(Game1.player.name + " ", "Farmer ");

                                        int hash = nextText.GetHashCode();
                                        ensureFolderStructureExists(Path.Combine(Path.Combine(tmppath, speakerName), "speech.mp3"));
                                        string file = Path.Combine(Path.Combine(tmppath, speakerName), "speech" + hash + ".mp3");
                                        FileInfo fileInfo = new FileInfo(file);


                                        if (!fileInfo.Exists)
                                        {

                                            SynthesizeSpeechRequest sreq = new SynthesizeSpeechRequest();
                                            sreq.Text = nextText;
                                            sreq.OutputFormat = OutputFormat.Mp3;
                                            sreq.VoiceId = currentVoice;
                                            SynthesizeSpeechResponse sres = pc.SynthesizeSpeech(sreq);

                                            using (var fileStream = File.Create(file))
                                            {
                                                sres.AudioStream.CopyTo(fileStream);
                                                fileStream.Flush();
                                                fileStream.Close();
                                            }
                                        }
                                        Monitor.Log(nextText);
                                        dl.RemoveAt(0);
                                        Helper.Reflection.GetPrivateField<List<string>>(d, "dialogues").SetValue(dl);
                                    }
                                    }catch
                                    {
                                        continue;
                                    }
                                    }
                            }
                            catch
                            {
                                continue;
                            }
                        }
                    }
                }*/
            }
        } 
    





    }
}
