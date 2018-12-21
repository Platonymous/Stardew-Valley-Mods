using StardewModdingAPI;
using StardewModdingAPI.Events;

using StardewValley;
using StardewValley.Menus;

using Amazon.Polly;
using Amazon.Polly.Model;

using Microsoft.Xna.Framework.Media;
using System.IO;

using System.Globalization;
using System.Collections.Generic;
using System.Threading;
using System;
using System.Text;
using Microsoft.Xna.Framework.Audio;
using System.Linq;

using OggSharp;


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


        private static AmazonPollyClient pc;
        private static VoiceId currentVoice;
        private static string tmppath;
        public static bool speak;
        private static string speakerName;
        private static SoundEffectInstance currentSpeech;

        public static IMonitor Monitor;

        public SpeechHandlerPolly()
        {

        }

        public static void start(IModHelper h)
        {
           
            Helper = h;
            currentText = "";
            tmppath = Path.Combine(Helper.DirectoryPath,"TTS");

            if (!Directory.Exists(tmppath))
                Directory.CreateDirectory(tmppath);

            pc = AWSHandler.getPollyClient();
            currentVoice = VoiceId.Amy;
            lastText = "";
            lastDialog = "";
            lastHud = "";
            speak = false;
            runSpeech = true;

            setVoice("default");

            speechThread = new Thread(t2sOut);
            speechThread.Start();
            h.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            h.Events.Display.MenuChanged += OnMenuChanged;

            //h.Events.Input.ButtonPressed += OnButtonPressed;
        }

        private static void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (e.Button == SButton.K)
            {
                gThread = new Thread(generateAllDialogs);
                gThread.Start();
            }
            

        }

        private static void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (e.NewMenu == null)
            {
                lastText = "";
                lastDialog = "";
                currentText = "";
                MediaPlayer.Stop();
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
            speakerName = name;

            string t = PelicanTTSMod.i18n.Get(name);
            if (t.ToString() == "")
                t = PelicanTTSMod.i18n.Get("default");

            if (VoiceId.FindValue(t) is VoiceId vId1)
                currentVoice = vId1;
            else if (VoiceId.FindValue(PelicanTTSMod.i18n.Get("default")) is VoiceId vId2)
                currentVoice = vId2;
            else
            {
                speakerName = "default";
                currentVoice = VoiceId.Salli;
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
                        setVoice(Game1.currentSpeaker.Name);
                    else
                        setVoice("default");

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
        }

        private static void t2sOut()
        {
            while (runSpeech)
            {
                try
                {
                    if (currentText == lastText) { continue; }

                    if (currentText.StartsWith("+"))
                        continue;

                    currentText = currentText.Replace('^', ' ').Replace(Environment.NewLine, " ").Replace("$s", "").Replace("$h", "").Replace("$g", "").Replace("$e", "").Replace("$u", "").Replace("$b", "").Replace("$8", "").Replace("$l", "").Replace("$q", "").Replace("$9", "").Replace("$a", "").Replace("$7", "").Replace("<", "").Replace("$r", "").Replace("[", "<").Replace("]", ">");

                    currentText = @"<speak>" + currentText + @"</speak>";
                    int hash = currentText.GetHashCode();
                    if (!Directory.Exists(Path.Combine(tmppath, speakerName)))
                        Directory.CreateDirectory(Path.Combine(tmppath, speakerName));

                    string file = Path.Combine(Path.Combine(tmppath, speakerName), "speech_" + PelicanTTSMod.config.Pitch + "_" + PelicanTTSMod.config.Volume + "_" + currentVoice.Value + "_" + hash + ".wav");
                    SoundEffect nextSpeech = null;

                    if (!File.Exists(file))
                    {
                        SynthesizeSpeechRequest sreq = new SynthesizeSpeechRequest();
                        sreq.Text = currentText;
                        sreq.TextType = TextType.Ssml;
                        sreq.OutputFormat = OutputFormat.Ogg_vorbis;
                        sreq.VoiceId = currentVoice;
                        SynthesizeSpeechResponse sres = pc.SynthesizeSpeech(sreq);
                        using (var memStream = new MemoryStream())
                        {
                            sres.AudioStream.CopyTo(memStream);
                            nextSpeech = Convert(memStream, file);
                        }
                    }
                    using (FileStream stream = new FileStream(file, FileMode.Open))
                        nextSpeech = SoundEffect.FromStream(stream);

                    if (currentSpeech != null)
                        currentSpeech.Stop();

                    currentSpeech = nextSpeech.CreateInstance();

                    if (Game1.activeClickableMenu is DialogueBox || Game1.hudMessages.Count > 0 || speak)
                    {
                        speak = false;
                        currentSpeech.Pitch = PelicanTTSMod.config.Pitch;
                        currentSpeech.Volume = PelicanTTSMod.config.Volume;
                        currentSpeech.Play();
                    }
                    lastText = currentText;
                }
                catch
                {
                    lastText = currentText;
                }
            }
        }

        public static SoundEffect Convert(Stream inputStream, string wavPath)
        {
            SoundEffect soundEffect = null;

            using (FileStream stream = new FileStream(wavPath, FileMode.Create))
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                OggDecoder decoder = new OggDecoder();
                decoder.Initialize(inputStream);
                byte[] data = decoder.SelectMany(chunk => chunk.Bytes.Take(chunk.Length)).ToArray();
                WriteWave(writer, decoder.Stereo ? 2 : 1, decoder.SampleRate, data);
            }

            using (FileStream stream = new FileStream(wavPath, FileMode.Open))
                soundEffect = SoundEffect.FromStream(stream);

            return soundEffect;
        }

        private static void WriteWave(BinaryWriter writer, int channels, int rate, byte[] data)
        {
            writer.Write(new char[4] { 'R', 'I', 'F', 'F' });
            writer.Write((36 + data.Length));
            writer.Write(new char[4] { 'W', 'A', 'V', 'E' });
            writer.Write(new char[4] { 'f', 'm', 't', ' ' });
            writer.Write(16);
            writer.Write((short)1);
            writer.Write((short)channels);
            writer.Write(rate);
            writer.Write((rate * ((16 * channels) / 8)));
            writer.Write((short)((16 * channels) / 8));
            writer.Write((short)16);
            writer.Write(new char[4] { 'd', 'a', 't', 'a' });
            writer.Write(data.Length);
            writer.Write(data);
        }

        private void WriteWavHeader(MemoryStream stream, bool isFloatingPoint, ushort channelCount, ushort bitDepth, int sampleRate, int totalSampleCount)
        {
            stream.Position = 0;
            stream.Write(Encoding.ASCII.GetBytes("RIFF"), 0, 4);
            stream.Write(BitConverter.GetBytes(((bitDepth / 8) * totalSampleCount) + 36), 0, 4);
            stream.Write(Encoding.ASCII.GetBytes("WAVE"), 0, 4);
            stream.Write(Encoding.ASCII.GetBytes("fmt "), 0, 4);
            stream.Write(BitConverter.GetBytes(16), 0, 4);
            stream.Write(BitConverter.GetBytes((ushort)(isFloatingPoint ? 3 : 1)), 0, 2);
            stream.Write(BitConverter.GetBytes(channelCount), 0, 2);
            stream.Write(BitConverter.GetBytes(sampleRate), 0, 4);
            stream.Write(BitConverter.GetBytes(sampleRate * channelCount * (bitDepth / 8)), 0, 4);
            stream.Write(BitConverter.GetBytes((ushort)channelCount * (bitDepth / 8)), 0, 2);
            stream.Write(BitConverter.GetBytes(bitDepth), 0, 2);
            stream.Write(Encoding.ASCII.GetBytes("data"), 0, 4);
            stream.Write(BitConverter.GetBytes((bitDepth / 8) * totalSampleCount), 0, 4);
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
                    
                            string file = Path.Combine(Path.Combine(tmppath, speakerName), "speech" + hash + ".wav");
                            FileInfo fileInfo = new FileInfo(file);


                            if (!fileInfo.Exists)
                            {

                                SynthesizeSpeechRequest sreq = new SynthesizeSpeechRequest();
                                sreq.Text = nextText;
                                sreq.OutputFormat = OutputFormat.Pcm;
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
                                    List<string> dl = Helper.Reflection.GetField<List<string>>(d, "dialogues").GetValue();
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
                                        Helper.Reflection.GetField<List<string>>(d, "dialogues").SetValue(dl);
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
