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
using System.Threading.Tasks;

namespace PelicanTTS
{
    internal class SpeechHandlerPolly
    {
        private static string lastText;
        private static string lastDialog;
        private static string lastLetter;
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
            currentVoice = VoiceId.Salli;
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
        }

        private static void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (e.NewMenu == null)
            {
                lastText = "";
                lastDialog = "";
                lastLetter = "";
                currentText = "";
                currentSpeech.Stop();
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
            if (e.IsMultipleOf(30))
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
                else if (Game1.activeClickableMenu is LetterViewerMenu lvm)
                {
                    setVoice("default");
                    List<string> mailMessage = Helper.Reflection.GetField<List<string>>(lvm, "mailMessage").GetValue();
                    string letter = mailMessage[Helper.Reflection.GetField<int>(lvm, "page").GetValue()];
                    if (letter != lastLetter)
                    {
                        currentText = letter;
                        lastLetter = letter;
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

                    if (Game1.activeClickableMenu is LetterViewerMenu || Game1.activeClickableMenu is DialogueBox || Game1.hudMessages.Count > 0 || speak)
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

                Thread.Sleep(500);
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
    }
}
