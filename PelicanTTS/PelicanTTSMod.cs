using StardewModdingAPI;
using StardewModdingAPI.Events;
using System;
using StardewValley;
using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using Harmony;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using StardewValley.Menus;
using Amazon.Polly;

namespace PelicanTTS
{
    public class PelicanTTSMod : Mod
    {
        internal static bool greeted;
        internal static ModConfig config;
        internal static IModHelper _helper;
        internal static IManifest _manifest;
        internal static ITranslationHelper i18n => _helper.Translation;
        internal static Dictionary<LocalizedContentManager.LanguageCode, List<string>> voices = new Dictionary<LocalizedContentManager.LanguageCode, List<string>>();
        internal static List<string> neural = new List<string>();
        internal static List<string> others = new List<string>();
        internal static List<string> conversational = new List<string>();
        internal static List<string> news = new List<string>();

        internal static string currentName = "Abigail";
        internal static Dictionary<object, int> currentIndex = new Dictionary<object, int>();
        internal static int maxValues = 5;
        internal static Screengraber screenGraber;

        internal static Dictionary<string, string> neuralReplacements;

        internal static Dictionary<string, Amazon.RegionEndpoint> endPoints;

        internal static Amazon.RegionEndpoint activeEndpoint = Amazon.RegionEndpoint.USEast1;


        public override void Entry(IModHelper helper)
        {
            endPoints = new Dictionary<string, Amazon.RegionEndpoint>()
            {
               { "USA",Amazon.RegionEndpoint.USEast1},
                {"UK",Amazon.RegionEndpoint.EUWest2},
                {"Germany",Amazon.RegionEndpoint.EUCentral1},
                {"Canada",Amazon.RegionEndpoint.CACentral1},
                {"Japan",Amazon.RegionEndpoint.APNortheast1}
            };

            _helper = helper;
            _manifest = ModManifest;
            config = Helper.ReadConfig<ModConfig>();
            activeEndpoint = endPoints[config.Server];
            screenGraber = new Screengraber();
            Helper.Events.GameLoop.OneSecondUpdateTicked += GameLoop_OneSecondUpdateTicked;

            helper.ConsoleCommands.Add("tts_update", "Updates new NPCs", (s, p) => setUpNPCConfig());

            Helper.WriteConfig<ModConfig>(config);
            string tmppath = Path.Combine(Path.Combine(Environment.CurrentDirectory, "Content"), "TTS");

            if (Directory.Exists(Path.Combine(Helper.DirectoryPath, "TTS")))
                if (!Directory.Exists(tmppath))
                    Directory.CreateDirectory(tmppath);

            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.GameLoop.ReturnedToTitle += OnReturnedToTitle;

            helper.Events.Input.ButtonPressed += Input_ButtonPressed;

            neural.AddRange(new string[]{
                "Kevin","Olivia",
            });

            neuralReplacements = new Dictionary<string, string>()
            {
                { "Kevin","Justin" },{"Olivia","Nicole"}
            };

            news.AddRange(new string[]{
                "Matthew", "Joanna", "Lupe", "Amy"
            });

            voices.Add(LocalizedContentManager.LanguageCode.de, (new List<string>()
            {
                "Marlene","Vicki","Hans"
            }).OrderBy(v => v).ToList());

            voices.Add(LocalizedContentManager.LanguageCode.en, (new List<string>()
            {
                "Brian","Amy","Joey","Emma","Nicole","Olivia","Justin","Russell","Joanna","Matthew","Kevin", "Kendra","Salli","Kimberly","Geraint","Ivy","Raveena","Aditi"
            }).OrderBy(v => v).ToList());

            voices.Add(LocalizedContentManager.LanguageCode.es, (new List<string>()
            {
                "Lucia","Conchita","Enrique",
                "Mia",
                "Penelope","Lupe","Miguel",
                "Vitoria","Camila","Ricardo","Cristiano","Ines",
                "Bianca","Carla","Giorgio"
            }).OrderBy(v => v).ToList());

            voices.Add(LocalizedContentManager.LanguageCode.pt, (new List<string>()
            {
                "Lucia","Conchita","Enrique",
                "Mia",
                "Penelope","Lupe","Miguel",
                "Vitoria","Camila","Ricardo","Cristiano","Ines"
            }).OrderBy(v => v).ToList());

            voices.Add(LocalizedContentManager.LanguageCode.fr, (new List<string>()
            {
                "Celine","Lea","Mathieu","Chantal"
            }).OrderBy(v => v).ToList());

            voices.Add(LocalizedContentManager.LanguageCode.it, (new List<string>()
            {
                "Bianca","Carla","Giorgio",
                "Mia",
                "Penelope","Lupe","Miguel",
                "Lucia","Conchita","Enrique"
            }).OrderBy(v => v).ToList());

            voices.Add(LocalizedContentManager.LanguageCode.ja, (new List<string>()
            {
                "Mizuki","Takumi"
            }).OrderBy(v => v).ToList());

            voices.Add(LocalizedContentManager.LanguageCode.ko, (new List<string>()
            {
                "Seoyeon"
            }).OrderBy(v => v).ToList());

            voices.Add(LocalizedContentManager.LanguageCode.ru, (new List<string>()
            {
                "Tatyana","Maxim"
            }).OrderBy(v => v).ToList());

            voices.Add(LocalizedContentManager.LanguageCode.tr, (new List<string>()
            {
                "Filiz"
            }).OrderBy(v => v).ToList());

            voices.Add(LocalizedContentManager.LanguageCode.zh, (new List<string>()
            {
                "Zhiyu"
            }).OrderBy(v => v).ToList());

            voices.Add(LocalizedContentManager.LanguageCode.hu, new List<string>());


            voices.Add(LocalizedContentManager.LanguageCode.th, new List<string>());


            others.AddRange(typeof(VoiceId).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static).Select(f => f.Name).Where(v => !voices.Any(vc => vc.Value.Contains(v))));

        }

        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (e.Button == config.ReadScreenKey && Game1.getMousePosition() is Point p)
                screenGraber.read(p);
        }

        private void GameLoop_OneSecondUpdateTicked(object sender, OneSecondUpdateTickedEventArgs e)
        {
            setUpNPCConfig();
            setUpConfig();
            Helper.Events.GameLoop.OneSecondUpdateTicked -= GameLoop_OneSecondUpdateTicked;

        }

        private void setUpNPCConfig()
        {
            var npcs = Helper.Content.Load<Dictionary<string, string>>("Data//NPCDispositions", ContentSource.GameContent);

            if (!config.Voices.ContainsKey("Default"))
                config.Voices.Add("Default", new VoiceSetup() { Voice = SpeechHandlerPolly.getVoice("default", true) });

            foreach (string npc in npcs.Keys)
            {
                if (!config.Voices.ContainsKey(npc))
                    config.Voices.Add(npc, new VoiceSetup() { Voice = SpeechHandlerPolly.getVoice(npc, npcs[npc].Contains("female")) });
            }
            config.Rate = Math.Max(50, Math.Min(config.Rate, 200));
            Helper.WriteConfig<ModConfig>(config);
        }
       
        public static Dictionary<string, MenuVoiceSetup> activeVoiceSetup = new Dictionary<string, MenuVoiceSetup>();
        public static int activeRate = 100;
        public static float activeVolume = 1;
        public void setUpConfig()
        {

            HarmonyInstance instance = HarmonyInstance.Create("PelicanTTS.GMCM");
           // instance.Patch(typeof(ChatBox).GetMethod("receiveChatMessage"), prefix: new HarmonyMethod(typeof(PelicanTTSMod).GetMethod("receiveChatMessage")));

            if (!Helper.ModRegistry.IsLoaded("spacechase0.GenericModConfigMenu"))
                return;

            var cVoices = voices[LocalizedContentManager.CurrentLanguageCode];
            var eVoices = new List<string>();

            foreach (var lang in voices.Where(v => v.Key != LocalizedContentManager.CurrentLanguageCode))
                foreach (var voice in lang.Value.Where(v => !cVoices.Contains(v)))
                    eVoices.Add(voice + "*");

            foreach (var voice in others.Where(v => !cVoices.Contains(v) && !eVoices.Contains(v + "*")))
                eVoices.Add(voice + "*");

            cVoices.AddRange(eVoices.OrderBy(v => v).Where(v => !cVoices.Contains(v)));

            maxValues = Math.Min(cVoices.Count + 1, 7);

            activeVoiceSetup = new Dictionary<string, MenuVoiceSetup>();
            var api = Helper.ModRegistry.GetApi<IGMCMAPI>("spacechase0.GenericModConfigMenu");

            api.RegisterModConfig(ModManifest, () =>
            {
                config.Greeting = true;
                config.MumbleDialogues = false;
                config.Pitch = 0;
                config.Volume = 1;
                config.Rate = 100;
                config.ReadDialogues = true;
                config.ReadHudMessages = true;
                config.ReadLetters = true;
                config.ReadNonCharacterMessages = true;
                config.ReadScreenKey = SButton.N;
                config.LanguageCode = SpeechHandlerPolly.getLanguageCode(true);
                //config.ReadChatMessages = true;

                var npcs = Helper.Content.Load<Dictionary<string, string>>("Data//NPCDispositions", ContentSource.GameContent);

                foreach (var voice in config.Voices.Keys)
                {
                    config.Voices[voice].Pitch = 0;
                    if (npcs.ContainsKey(voice))
                        config.Voices[voice].Voice = SpeechHandlerPolly.getVoice(voice, npcs[voice].Contains("female"));
                }
            }, () => Helper.WriteConfig<ModConfig>(config));
            api.RegisterLabel(ModManifest, MainLabelText, "");

            api.RegisterSimpleOption(ModManifest, "Mumbling", "Should all NPCs mumble", () => config.MumbleDialogues, (s) => config.MumbleDialogues = s);
            api.RegisterSimpleOption(ModManifest, "Greeting", "Enables the morning greeting", () => config.Greeting, (s) => config.Greeting = s);
            api.RegisterSimpleOption(ModManifest, "Read Character Dialogues", "", () => config.ReadDialogues, (s) => config.ReadDialogues = s);
            api.RegisterSimpleOption(ModManifest, "Read Non-Character Messages", "", () => config.ReadNonCharacterMessages, (s) => config.ReadNonCharacterMessages = s);
            api.RegisterSimpleOption(ModManifest, "Read Letters", "", () => config.ReadLetters, (s) => config.ReadLetters = s);
            api.RegisterSimpleOption(ModManifest, "Read Hud Messages", "", () => config.ReadHudMessages, (s) => config.ReadHudMessages = s);
            api.RegisterSimpleOption(ModManifest, "Read Screen Key", "", () => config.ReadScreenKey, (s) => config.ReadScreenKey = s);
            api.RegisterSimpleOption(ModManifest, "Language Code", "", () => config.LanguageCode, (s) => config.LanguageCode = s);
            //  api.RegisterSimpleOption(ModManifest, "Read Chat Messages", "", () => config.ReadChatMessages, (s) => config.ReadChatMessages = s);
            api.RegisterSimpleOption(ModManifest, "Neural Voices", "", () => config.UseNeuralVoices, (s) => config.UseNeuralVoices = s);

            api.RegisterChoiceOption(ModManifest, "Server", "", () => config.Server, (s) =>
            {
                config.Server = s;
                activeEndpoint = endPoints[s];
                SpeechHandlerPolly.pc = null;
            }, endPoints.Keys.ToArray());

            api.RegisterClampedOption(ModManifest, "Volume", "Set Volume", () =>
            {
                activeVolume = config.Volume;
                return    config.Volume;
            }, (s) =>
            {
                config.Volume = (float)Math.Ceiling((double)(s * 100)) / 100f;
            }, 0, 1);

            api.RegisterClampedOption(ModManifest, "Rate", "Set Rate (20-200%)", () =>
            {
                activeRate = config.Rate;
                return config.Rate;
            }, (s) =>
            {
                config.Rate = (int) s;
            }, 50, 200);

            /*    instance.Patch(Type.GetType("GenericModConfigMenu.UI.Dropdown, GenericModConfigMenu").GetMethod("Update"), new HarmonyMethod(typeof(PelicanTTSMod).GetMethod("UpdateGMCM")));
                instance.Patch(Type.GetType("GenericModConfigMenu.UI.Dropdown, GenericModConfigMenu").GetMethod("Draw"), new HarmonyMethod(typeof(PelicanTTSMod).GetMethod("DrawGMCM")));
                instance.Patch(Type.GetType("GenericModConfigMenu.UI.Table, GenericModConfigMenu").GetMethod("Update"), new HarmonyMethod(typeof(PelicanTTSMod).GetMethod("UpdateTableGMCM")));
                instance.Patch(Type.GetType("GenericModConfigMenu.UI.Scrollbar, GenericModConfigMenu").GetMethod("Scroll"), new HarmonyMethod(typeof(PelicanTTSMod).GetMethod("ScrollGMCM")));
            */

            Helper.Events.Input.MouseWheelScrolled += (s, e) =>
                {
                    List<object> obj = new List<object>();
                    foreach(var dropDown in currentIndex.Keys.Where(k => ((bool)k.GetType().GetField("dropped").GetValue(k))))
                    {
                        obj.Add(dropDown);
                        currentIndex[dropDown] -= e.Delta / 120;
                        currentIndex[dropDown] = Math.Max(currentIndex[dropDown],0);
                        break;
                    }
                };

                api.RegisterLabel(ModManifest, "Voices", "Set the voices for each NPC");
                int index = 2;

                foreach (var npc in config.Voices.Keys.OrderBy(k => k))
                {
                    if (!activeVoiceSetup.ContainsKey(npc))
                        activeVoiceSetup.Add(npc, new MenuVoiceSetup());
                    activeVoiceSetup[npc].Index = index;
                    activeVoiceSetup[npc].Name = npc;

                    List<string> npcVoices = new List<string>() { npc + ":default" };
                    npcVoices.AddRange(cVoices);
                    api.RegisterChoiceOption(ModManifest, npc + " Voice", "Choose a voice", () =>
                    {
                        Game1.stopMusicTrack(Game1.MusicContext.Default);
                        if (config.Voices[npc].Voice.Contains("default"))
                            return npc + ":default";

                        activeVoiceSetup[npc].Voice = config.Voices[npc].Voice;
                        string voice = config.Voices[npc].Voice;

                        if (!voices[LocalizedContentManager.CurrentLanguageCode].Contains(voice))
                            return voice + "*";
                        return voice;
                    }, (s) =>
                    {
                        config.Voices[npc].Voice = s.Replace(npc + ":", "").Replace("*","");
                        activeVoiceSetup[npc].Voice = config.Voices[npc].Voice;
                        currentName = npc;
                    }, npcVoices.ToArray());

                    api.RegisterClampedOption(ModManifest, npc + " Pitch", "Choose a Pitch", () => config.Voices[npc].Pitch, (s) =>
                    {
                        config.Voices[npc].Pitch = (float)Math.Ceiling((double)(s * 100)) / 100f;
                        activeVoiceSetup[npc].Pitch = config.Voices[npc].Pitch;
                    }, -1, 1);

                    index++;
                }

                for (int i = 0; i < 12; i++)
                    api.RegisterLabel(ModManifest, " ", " ");

            api.SubscribeToChange(ModManifest, new Action<string, string>((key, value) =>
            {
                if (key.EndsWith("Voice"))
                {
                    string character = key.Split(' ')[0];

                    if (value == "Default")
                        value = SpeechHandlerPolly.getVoice(character);
                    var mvs = activeVoiceSetup.Values.FirstOrDefault(avs => avs.Name == character);
                    value = value.Replace("*", "");
                    var intro = character;
                    if (intro == "Default")
                        intro = i18n.Get("FestivalGreeting");
                    else
                    {
                        var dialogues = _helper.Content.Load<Dictionary<string, string>>(@"Characters/Dialogue/" + character, ContentSource.GameContent);
                        if (dialogues != null && dialogues.ContainsKey("Introduction"))
                        {
                            intro = dialogues["Introduction"].Split('^')[0].Split('#')[0].Replace("@", "");
                            if(intro.Length < 7)
                                intro = dialogues["Mon"].Split('^')[0].Split('#')[0].Replace("@", "");
                        }
                    }
                    SpeechHandlerPolly.configSay(character, value, intro, activeRate, mvs is MenuVoiceSetup ? mvs.Pitch : -1, activeVolume);
                }

            }));

            api.SubscribeToChange(ModManifest, new Action<string, int>((key, value) =>
            {
                if (key.Contains("Rate"))
                    activeRate = value;
            }));

            api.SubscribeToChange(ModManifest, new Action<string, float>((key, value) =>
            {
                if (key.EndsWith("Pitch"))
                {
                    string character = key.Split(' ')[0];
                    var mvs = activeVoiceSetup.Values.First(avs => avs.Name == character);
                    mvs.Pitch = (float)Math.Ceiling((double)(value * 100)) / 100f;
                }
                else if (key.Contains("Volume"))
                    activeVolume = (float)Math.Ceiling((double)(value * 100)) / 100f;
            }));
        }

        public static void receiveChatMessage(ChatBox __instance, long sourceFarmer, int chatKind, LocalizedContentManager.LanguageCode language, string message)
        {
            if (Game1.player.UniqueMultiplayerID != sourceFarmer && (chatKind == 3 || chatKind == 0 ))
                SpeechHandlerPolly.chats.Enqueue(message);
        }

        public static bool ScrollGMCM(object __instance)
        {
            return Mouse.GetState().LeftButton != ButtonState.Pressed;
        }

        public static bool DrawGMCM(object __instance, SpriteBatch b)
        {
            if (!IsThisPage)
                return true;

            List<string> choicesc = new List<string>((string[])__instance.GetType().GetProperty("Choices").GetValue(__instance));
            if (!choicesc[0].Contains(":default"))
                return true;

            Vector2 pos = (Vector2)__instance.GetType().GetProperty("Position").GetValue(__instance);
            Texture2D tex = (Texture2D)__instance.GetType().GetProperty("Texture").GetValue(__instance);
            Microsoft.Xna.Framework.Rectangle rect = (Microsoft.Xna.Framework.Rectangle)__instance.GetType().GetProperty("BackgroundTextureRect").GetValue(__instance);
            Microsoft.Xna.Framework.Rectangle rect2 = (Microsoft.Xna.Framework.Rectangle)__instance.GetType().GetProperty("ButtonTextureRect").GetValue(__instance);
            string[] choices = (string[])__instance.GetType().GetProperty("Choices").GetValue(__instance);
            string value = (string)__instance.GetType().GetProperty("Value").GetValue(__instance);
            int aChoice = (int)__instance.GetType().GetProperty("ActiveChoice").GetValue(__instance);
            bool dropped = (bool)__instance.GetType().GetField("dropped").GetValue(__instance);

            IClickableMenu.drawTextureBox(b, tex, rect, (int)pos.X, (int)pos.Y, 252, 44, Color.White, 4f, false);
            b.DrawString((SpriteFont)Game1.smallFont, value, new Vector2(pos.X + 4f, pos.Y + 8f), (Color)Game1.textColor);
            b.Draw(tex, new Vector2((float)((double)pos.X + 300.0 - 48.0), pos.Y), new Microsoft.Xna.Framework.Rectangle?(rect2), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0f);
            
            if (!dropped)
                return true;

            if (!currentIndex.ContainsKey(__instance))
                currentIndex.Add(__instance, 0);

            currentIndex[__instance] = Math.Min(Math.Max(Math.Min(currentIndex[__instance], choices.Length - 1), 0), choices.Length - maxValues);
            int num = maxValues * 44;
            IClickableMenu.drawTextureBox(b, tex, rect, (int)pos.X, (int)pos.Y, 252, num, Color.White, 4f, false);
            int cIndex = 0;
            for (int index = currentIndex[__instance]; index < Math.Min(choices.Length, currentIndex[__instance] + maxValues); ++index)
            {

                if (index == aChoice)
                    b.Draw((Texture2D)Game1.staminaRect, new Microsoft.Xna.Framework.Rectangle((int)pos.X + 4, (int)pos.Y + cIndex * 44, 244, 44), new Microsoft.Xna.Framework.Rectangle?(), Color.Wheat, 0.0f, Vector2.Zero, SpriteEffects.None, 0.98f);
                
                b.DrawString((SpriteFont)Game1.smallFont, choices[index], new Vector2(pos.X + 4f, (float)((double)pos.Y + (double)(cIndex * 44) + 8.0)), (Color)Game1.textColor, 0.0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
                cIndex++;
            }

            return false;
        }

        public static int lastActive = 0;
        public static bool exiting = false;
        public static bool IsThisPage = false;
        const string MainLabelText = "Pelican TTS";
        public static bool UpdateTableGMCM(object __instance, ref bool __state)
        {
           List<object> children = new List<object>((IList<object>) __instance.GetType().GetProperty("Children").GetValue(__instance));

            object mainLabel = children.FirstOrDefault(c => c.GetType().Name.Contains("Label"));
            if (mainLabel == null)
            {
                IsThisPage = false;
                return true;
            }
            object mainLabelText = mainLabel.GetType().GetProperty("String").GetValue(mainLabel);
            IsThisPage = mainLabelText is string mlt && mlt == MainLabelText;

            if (!IsThisPage)
                return true;

            int index = 0;
            foreach (var child in children.Where(c => c.GetType().Name.Contains("Slider"))){
                if(index < 2)
                {
                    if (index == 1)
                    {
                        int rvalue = (int)child.GetType().GetProperty("Value").GetValue(child);
                        activeRate = rvalue;
                    }
                    else
                    {
                        float vvalue = (float)child.GetType().GetProperty("Value").GetValue(child);
                        activeVolume = (float)Math.Ceiling((double)(vvalue * 100)) / 100f;
                    }

                    index++;
                    continue;
                }

                float value = (float)child.GetType().GetProperty("Value").GetValue(child);
                    foreach (var avs in activeVoiceSetup.Values.Where(av => av.Index == index))
                        avs.Pitch = value;
                index++;
            }
            return true;
        }

        public static bool UpdateGMCM(object __instance, ref bool __state)
        {
            if (!IsThisPage)
                return true;

            List<string> choices = new List<string>((string[])__instance.GetType().GetProperty("Choices").GetValue(__instance));
            if (!choices[0].Contains(":default"))
                return true;

            if (!currentIndex.ContainsKey(__instance))
                currentIndex.Add(__instance, 0);

            bool dropped = (bool)__instance.GetType().GetField("dropped").GetValue(__instance);
            Vector2 pos = (Vector2)__instance.GetType().GetProperty("Position").GetValue(__instance);
            int aChoice = (int)__instance.GetType().GetProperty("ActiveChoice").GetValue(__instance);
            object callback = __instance.GetType().GetField("Callback").GetValue(__instance);
            object parent = __instance.GetType().GetProperty("Parent").GetValue(__instance);


            MouseState state;
            int num;

            if (new Rectangle((int)pos.X, (int)pos.Y, 300, 44).Contains(Game1.getOldMouseX(), Game1.getOldMouseY()) && Game1.oldMouseState.LeftButton == ButtonState.Released)
            {
                state = Mouse.GetState();
                num = state.LeftButton == ButtonState.Pressed ? 1 : 0;
            }
            else
                num = 0;
            if (num != 0)
            {
                __instance.GetType().GetField("dropped").SetValue(__instance, true);
                parent.GetType().GetProperty("RenderLast").SetValue(parent,__instance);
            }
            if (!dropped)
                currentIndex[__instance] = Math.Min(aChoice, choices.Count - maxValues);

            if (!dropped)
                return false;
            state = Mouse.GetState();
            if (state.LeftButton == ButtonState.Released)
            {
                __instance.GetType().GetField("dropped").SetValue(__instance, false);
                if (parent.GetType().GetProperty("RenderLast").GetValue(parent) == __instance)
                    parent.GetType().GetProperty("RenderLast").SetValue(parent,null);

                string value = (string)__instance.GetType().GetProperty("Value").GetValue(__instance);
                string cname = choices[0].Split(':')[0];
                if (value == choices[0])
                    value = SpeechHandlerPolly.getVoice(cname);
                var mvs = activeVoiceSetup.Values.First(avs => avs.Name == choices[0].Split(':')[0]);

                var intro = choices[0].Split(':')[0];
                if (intro == "Default")
                    intro = i18n.Get("FestivalGreeting");
                else
                {
                    var dialogues = _helper.Content.Load<Dictionary<string, string>>(@"Characters/Dialogue/" + intro, ContentSource.GameContent);
                    if (dialogues != null && dialogues.ContainsKey("Introduction"))
                        intro = dialogues["Introduction"].Split('^')[0].Split('#')[0].Replace("@", "");
                }
                SpeechHandlerPolly.configSay(choices[0].Split(':')[0], value, intro, activeRate, mvs is MenuVoiceSetup ? mvs.Pitch : -1, activeVolume);
            }

            if (new Rectangle((int)pos.X, (int)pos.Y, 300, 44 * maxValues).Contains(Game1.getOldMouseX(), Game1.getOldMouseY()))
            {
                int idx = ((Game1.getOldMouseY() - (int)pos.Y) / 44) + currentIndex[__instance];
                idx = Math.Min(idx, choices.Count - 1);
                __instance.GetType().GetProperty("ActiveChoice").SetValue(__instance,idx);
                if (callback != null)
                {
                    try
                    {
                        callback.GetType().GetMethod("Invoke").Invoke(callback, new[] { __instance });
                    }
                    catch
                    {

                    }
                }
            }

            return false;
        }


        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (e.IsOneSecond && !greeted && Game1.timeOfDay == 600 && Game1.activeClickableMenu == null)
            {
                performGreeting();
                greeted = true;
            }
        }


        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            greeted = false;
        }

        private static string dayNameFromDayOfSeason(int dayOfSeason)
        {
            switch (dayOfSeason % 7)
            {
                case 0:
                    return i18n.Get("Sunday");
                case 1:
                    return i18n.Get("Monday");
                case 2:
                    return i18n.Get("Tuesday");
                case 3:
                    return i18n.Get("Wednesday");
                case 4:
                    return i18n.Get("Thursday");
                case 5:
                    return i18n.Get("Friday");
                case 6:
                    return i18n.Get("Saturday");
                default:
                    return "";
            }
        }

        private void performGreeting()
        {
            if (!config.Greeting)
                return;

            NPC birthday = Utility.getTodaysBirthdayNPC(Game1.currentSeason, Game1.dayOfMonth);
            string day = Game1.dayOfMonth.ToString();

            string greeting = i18n.Get("Greeting").ToString().Replace("{Player}", Game1.player.Name).Replace("{DayName}", dayNameFromDayOfSeason(Game1.dayOfMonth)).Replace("{Day}", @"[say-as interpret-as='date']??????" + (day.Length < 2 ? "0"+day : day) + @"[/say-as]").Replace("{Season}", i18n.Get(Game1.currentSeason)) + " ";
            if (birthday != null)
            {
                string person = birthday.Name;
                if (birthday == Game1.player.getSpouse())
                    if (birthday.Gender == 0)
                        person = i18n.Get("Your husband");
                    else
                        person = i18n.Get("Your wife");

                greeting += i18n.Get("BirthdayGreeting").ToString().Replace("{NPC}",person);
            }

            if (Utility.isFestivalDay(Game1.dayOfMonth, Game1.currentSeason))
            {
                int festivalTime = Utility.getStartTimeOfFestival();
                int ftHours = (int)Math.Floor((double)festivalTime / 100);
                int ftMinutes = festivalTime - (ftHours * 100);
                string timeInfo = i18n.Get("a.m.");
                if (ftHours > 12)
                {
                    ftHours -= 12;
                    timeInfo = i18n.Get("p.m.");
                }

                greeting += i18n.Get("FestivalGreeting") + " " + ftHours + " " + timeInfo + ".";
            }


            say(greeting);
        }



        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
        }

        private void OnReturnedToTitle(object sender, ReturnedToTitleEventArgs e)
        {
            Helper.Events.GameLoop.UpdateTicked -= OnUpdateTicked;
            Helper.Events.Input.ButtonPressed -= OnButtonPressed;

            SpeechHandlerPolly.stop();
        }

        
        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            Helper.Events.GameLoop.DayStarted += OnDayStarted;
            SpeechHandlerPolly.Monitor = Monitor;
            SpeechHandlerPolly.start(Helper);

            Helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            Helper.Events.Input.ButtonPressed += OnButtonPressed;
        }


        public static void say(string text)
        {
            SpeechHandlerPolly.lastSay = text;
            SpeechHandlerPolly.currentText = text;
            SpeechHandlerPolly.speak = true;
        }
    }
}
