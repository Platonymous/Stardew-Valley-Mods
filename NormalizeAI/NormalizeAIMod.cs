using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using static StardewValley.Minigames.BoatJourney;
using System.Net;
using System.IO;
using System.Text.Json.Serialization;
using System.Collections;
using Microsoft.Xna.Framework.Input;
#if DEBUG || RELEASE
using StardewValley.Extensions;
#endif
using static StardewValley.Menus.CharacterCustomization;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Specialized;
using Sickhead.Engine.Util;

namespace NormalizeAI
{
    public class CPEntry
    {
        public IContentPack ContentPack { get; set; }

        public string Source { get; set; }
    
    }

    public class DialogueEntry
    {
        public NPC NPC { get; set; }

        public string Message { get; set; }
    }

    public class NormalizeAIMod : Mod
    {
        public static Queue<DialogueEntry> DialogueQueue { get; set; } = new Queue<DialogueEntry>();
        public static bool ActiveDialogue { get; set; } = false;

        public static bool Wait { get; set; } = false;

        public static Action RunPrompt { get; set; }

        public static NPC LastNPC { get; set; }

        public static object InworldMod { get; set; }

        public const string AIIndicator = "Platonymous.NormalizeAI.Auto";

        public const string AIDialogueIndicator = "NAI_Prompt:";
        public const string AIDialogueIndicatorEvent = "NAI_Prompt";
        public const string AIDialougeActive = "Platonymous.NormalizeAI";
        public const string LOADDIALOG = ". . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . .";

        public static List<CPEntry> PacksLoaded { get; set; } = new List<CPEntry>();
        public static Dictionary<string, object> AdditionalBrains { get; } = new Dictionary<string, object>();

        public Random Random { get; set; } = new Random();

        public static NormalizeAIMod Singleton { get; set; }

        public static int WaitingForEvent { get; set; } = -1;

        public override void Entry(IModHelper helper)
        {
            Singleton = this;

            Harmony harmony = new Harmony("Platonymous.NormalizeAI");

            {
                harmony.Patch(
                    original: AccessTools.Method(Type.GetType("Inworld.Mod, Inworld"), "TryToInit"),
                    prefix: new HarmonyMethod(typeof(NormalizeAIMod), nameof(GetInit)));

                harmony.Patch(
                    original: AccessTools.Method(Type.GetType("Inworld.Mod, Inworld"), "LogMessage"),
                    prefix: new HarmonyMethod(typeof(NormalizeAIMod), nameof(SendResponseToDialouge)));


                harmony.Patch(
                    original: AccessTools.Method(Type.GetType("Inworld.Mod, Inworld"), "ChatBox_OnEnterPressed"),
                    prefix: new HarmonyMethod(typeof(NormalizeAIMod), nameof(ChatBox_OnEnterPressed)));

                harmony.Patch(
                    original: AccessTools.Method(Type.GetType("Inworld.Mod, Inworld"), "DisplayMessage"),
                    prefix: new HarmonyMethod(typeof(NormalizeAIMod), nameof(BlockMessage)));


                helper.Events.GameLoop.OneSecondUpdateTicked += GameLoop_OneSecondUpdateTicked;

                Monitor.Log("Patched Inworld", LogLevel.Trace);
            }

            {
                harmony.Patch(
                original: AccessTools.PropertyGetter(typeof(NPC), nameof(NPC.CurrentDialogue)),
                postfix: new HarmonyMethod(typeof(NormalizeAIMod), nameof(SetCurrentDialogue))
                );

                helper.Events.Display.MenuChanged += Display_MenuChanged;
                helper.Events.Player.Warped += Player_Warped;
            }

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;

            helper.Events.Content.AssetRequested += Content_AssetRequested;
        }

        public static void ChatBox_OnEnterPressed(object __instance, TextBox sender)
        {
            if(sender != null)
            {
                string target = AccessTools.Field(InworldMod.GetType(), "Target").GetValue(__instance).ToString();
                if (Game1.getCharacterFromName(target) is NPC npc)
                {
                    string prompt = GetTranslated(GetContext(GetMoodPrompt(AccessTools.Field(__instance.GetType(), "TextInput").GetValue(__instance).ToString()), npc));
                    AccessTools.Field(__instance.GetType(), "TextInput").SetValue(InworldMod, prompt);
                }

            }
        }

        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            return;

            if (e.NameWithoutLocale.IsEquivalentTo("Characters/Dialogue/Abigail"))
            {
                e.Edit(asset =>
                {
                    asset.AsDictionary<string, string>().Data.Add("summer_2", AIDialogueIndicator + "Tell me what you think about Sam's hair");
                });
            }
            if (e.NameWithoutLocale.IsEquivalentTo("Data/Events/SeedShop"))
            {
                e.Edit(asset =>
                {
                    asset.AsDictionary<string, string>().Data["3102768/j 15/H"] = asset.AsDictionary<string, string>().Data["3102768/j 15/H"].Replace("speak Pierre \"Welcome To Pierre's! How can I help you?\"",$"{AIDialogueIndicatorEvent} Pierre Tell me about Sam's hair");
                });
            }

        }

        

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            foreach(var cp in Helper.ContentPacks.GetOwned())
            {
                string source = File.ReadAllText(Path.Combine(cp.DirectoryPath, "config.json"));
                PacksLoaded.Add(new CPEntry() { ContentPack = cp, Source = source });
            }

#if DEBUG || RELEASE
            Event.RegisterCommand(AIDialogueIndicatorEvent, (e, s, c) =>
            {
                if (WaitingForEvent == -1)
                {
                    List<string> list = new List<string>(s);
                    NPC character = Game1.getCharacterFromName(list[1]);
                    list.RemoveAt(0);
                    list.RemoveAt(0);

                    var prompt = GetTranslated(GetContext(GetMoodPrompt(string.Join(' ', list).Replace(AIDialogueIndicatorEvent, "")), character));
                    SetPrompt(prompt, character);
                    RunPrompt();
                    WaitingForEvent = 4000;
                }
                else
                {
                    if (Game1.activeClickableMenu == null)
                        WaitingForEvent--;

                    if (WaitingForEvent <= 0)
                    {
                        WaitingForEvent = -1;
                        e.CurrentCommand++;
                    }
                }
            });
#endif
        }

        private void Player_Warped(object sender, StardewModdingAPI.Events.WarpedEventArgs e)
        {
            LastNPC = null;
        }

        private void Display_MenuChanged(object sender, StardewModdingAPI.Events.MenuChangedEventArgs e)
        {
#if DEBUG || RELEASE
            if (e.NewMenu is DialogueBox db && db.characterDialogue?.TranslationKey == AIIndicator)
#else
                if (e.NewMenu is DialogueBox db && db.characterDialogue?.temporaryDialogueKey is string td && td == AIIndicator)

#endif
            {
                var prompt = GetTranslated(GetContext(GetRandomPrompt(db.characterDialogue.speaker), db.characterDialogue.speaker));
                SetPrompt(prompt, db.characterDialogue.speaker);
                RunPrompt();
            }
            else if (e.NewMenu is DialogueBox db2 && string.Join(' ', db2.characterDialogue?.dialogues?.ToArray() ?? new string[0]) is string dia && !string.IsNullOrEmpty(dia) && dia.Contains(AIDialogueIndicator))
            {
#if DEBUG || RELEASE
                var dialogue = new Dialogue(db2.characterDialogue.speaker, AIDialogueIndicator, LOADDIALOG);
#else
                var dialogue = new Dialogue(LOADDIALOG, db2.characterDialogue.speaker);
                dialogue.temporaryDialogueKey = AIDialogueIndicator;

#endif
                Game1.activeClickableMenu = new DialogueBox(dialogue);
                var prompt = GetTranslated(GetContext(GetMoodPrompt(dia.Replace(AIDialogueIndicator, "").Replace(AIDialogueIndicator+" ", "")), db2.characterDialogue.speaker));
                SetPrompt(prompt, db2.characterDialogue.speaker);
                RunPrompt();
            }
        }

        public string GetRandomPrompt(NPC npc)
        {
            bool followUp = npc == LastNPC;
            LastNPC = npc;

            if (followUp)
                return GetRandomFollowUpPrompt();
            else
                return GetRandomInitialPrompt();
        }

        public string GetRandomInitialPrompt()
        {
            string[] prompts = new[] {
                "Talk about the day, the weather, the time, and/or the season",
                "Talk about someone who is also in this place",
                "Talk about your day, as a statement",
                "Tell me something that angers you",
                "Tell me something that you love",
                "Tell me something sad",
                "Think of a question, answer it without repeating it, weave it into your answer",
                "Talk about your plans for today, as a statement",
                "Tell me something personal, as a statement",
                "Talk about someone else, as a statement",
                "Share gossip, as a statement",
                "Talk about me, as a statement to me",
                "Talk about something that happend a few days ago",
                "Talk about something that happend to someone else",
            };

            string[] weather = new[]
            {
                "",
                ",do not talk about the weather, the season or the time",
                ",do not talk about the weather or the season",
                ",do not talk about the weather or the time"
            };

            var pnum = Random.Next(0, prompts.Length);

            if (pnum == 1 && Game1.currentLocation.characters.Where(c => c.isVillager()).Select(v => v.Name).Count() < 1)
                pnum++;

            return GetExtendedPrompt(prompts[pnum] + weather[pnum > 0 ? Random.Next(0, weather.Length) : 0]);
        }



        public static string GetContext(string p, NPC npc)
        {
            bool isDivorced = npc.isDivorcedFrom(Game1.player);
            bool isMarried = Game1.player.spouse == npc.Name;
            string inLocation = Game1.currentLocation.Name;
            bool isHome = Game1.currentLocation == npc.getHome();
#if DEBUG || RELEASE
            bool isRaining = Game1.getLocationFromName("Town").IsRainingHere();
#else
            bool isRaining = Game1.isRaining;
#endif
            bool outside = Game1.currentLocation.IsOutdoors;
            bool birthday = npc.Birthday_Day == Game1.dayOfMonth && npc.Birthday_Season == Game1.currentSeason;
            var context = new List<string>();
            var fp = Game1.player.getFriendshipHeartLevelForNPC(npc.Name);

            if (!isDivorced && !isMarried)
                if (fp < 2)
                    context.Add("you are barely know the player");
                else if (fp < 3)
                    context.Add("know the player but no friends yet");
                else if (fp < 6)
                    context.Add("you are friends with the player");
                else if (fp < 10)
                    context.Add("you are close friends with the player");
                else if (fp > 10)
                    context.Add("you are very close friends with the player");

            var others = Game1.currentLocation.characters.Where(c => c.isVillager()).Select(v => v.Name).ToArray();

            if (others.Length > 0)
                context.Add($"there are {others.Length} ther people int the same location: {string.Join(',', others)}");

            if (isHome)
                context.Add("you are inside your home");
            else
                context.Add($"you are {(outside ? "outside" : "inside")} in the {inLocation} Location");

            if (isMarried)
                context.Add("you are married to the player");

            if (isDivorced)
                context.Add("you are divorced from the player");

            if (birthday)
                context.Add("it is your birthday");
#if DEBUG || RELEASE
            context.Add($"it's {Game1.shortDayNameFromDayOfSeason}. the {Game1.dayOfMonth}. of {Game1.currentSeason}");
#else
            context.Add($"it's {Game1.currentSeason}. the {Game1.dayOfMonth}. of {Game1.currentSeason}");
#endif

            if (isRaining)
                context.Add($"it is raining{(outside ? " outside" : "")}");

            var timeDay = Game1.timeOfDay.ToString().ToCharArray();
            string am = "am";

            var time = Game1.timeOfDay.ToString();

            if (Game1.timeOfDay > 1200)
            {
                timeDay = (Game1.timeOfDay - 1200).ToString().ToCharArray();
                am = "pm";
            }

            if (timeDay.Length == 3)
                time = $"{timeDay[0]}:{timeDay[1]}{timeDay[2]} {am}";
            else if (timeDay.Length == 4)
                time = $"{timeDay[0]}{timeDay[1]}:{timeDay[2]}{timeDay[3]} {am}";

            context.Add($"it is {time}");

            var pnew = $"(Context: {string.Join(',', context)}) {p}";

            return pnew;
        }

        public static string GetTranslated(string p)
        {
            if (Singleton.Helper.Translation.LocaleEnum == LocalizedContentManager.LanguageCode.en)
                return p;

            var plang = $"Only respond in the this language: {GetLanguage()}, Prompt:{p}";

            return plang;
        }

        public static string GetLanguage()
        {
            switch (Singleton.Helper.Translation.LocaleEnum)
            {
                case LocalizedContentManager.LanguageCode.en: return "english";
                case LocalizedContentManager.LanguageCode.pt: return "portuguese";
                case LocalizedContentManager.LanguageCode.es: return "spanish";
                case LocalizedContentManager.LanguageCode.fr: return "frensh";
                case LocalizedContentManager.LanguageCode.de: return "german";
                case LocalizedContentManager.LanguageCode.hu: return "hungarian";
                case LocalizedContentManager.LanguageCode.it: return "italian";
                case LocalizedContentManager.LanguageCode.ja: return "japanese";
                case LocalizedContentManager.LanguageCode.ko: return "korean";
                case LocalizedContentManager.LanguageCode.mod: return Singleton.Helper.Translation.Locale;
                case LocalizedContentManager.LanguageCode.th: return "thai";
                case LocalizedContentManager.LanguageCode.ru: return "russian";
                case LocalizedContentManager.LanguageCode.tr: return "turkish";
                case LocalizedContentManager.LanguageCode.zh: return "chinese";
                default: return Singleton.Helper.Translation.Locale;
            }
        }

        public static string GetExtendedPrompt(string p)
        {
            return GetMoodPrompt(p + ", without acknowlaging my request");
        }
        public static string GetMoodPrompt(string p)
        {
            return p + ",if you talk something happy prefix you response with ' $h ', if you talk something sad with ' $s ', if you talk something you love with ' $l ', else without prefix";
        }

        public string GetRandomFollowUpPrompt()
        {
            string[] prompts = new[] {
                "Tell me more",
                "What angers you about that",
                "Tell me something you love about that",
                "Tell me something sad about that",
                "Tell what another person thought about that",
                "Think of a followup question, answer it without repeating it but instead weave it into your answer",
                "Ask me a question about that",
                "Talk about something completely different",
                "Talk about me, as a statement to me",
                "Talk about a past story that relates to this",
                "Talk about future plans that relate to this",
                "X"
                };

            string[] weather = new[]
            {
                ",do not talk about the weather, the season or the time",
                ",do not talk about the weather or the season",
                ",do not talk about the weather or the time",
                ""
            };

            var p = prompts[Random.Next(0, prompts.Length)];

            if (p == "X")
                return GetRandomInitialPrompt();
            else
                return GetExtendedPrompt(p + weather[Random.Next(0, weather.Length)]);
        }

        public static void SetCurrentDialogue(NPC __instance, ref Stack<Dialogue> __result)
        {
            if(!Game1.eventUp && (__result == null || __result.Count == 0))
            {
#if DEBUG || RELEASE
                var dialogue = new Dialogue(__instance, AIIndicator, LOADDIALOG);
#else
                var dialogue = new Dialogue(LOADDIALOG, __instance);
                dialogue.temporaryDialogueKey = AIIndicator;
#endif
                __result = new Stack<Dialogue>(new[] { dialogue });
            }
        }

        public static void SetPrompt(string prompt, NPC npc)
        {
            AccessTools.Field(InworldMod.GetType(), "TextInput").SetValue(InworldMod, prompt);
            AccessTools.Field(InworldMod.GetType(), "Target").SetValue(InworldMod, npc.displayName);

        }

        public static bool AreComparableBrains(object brain1, object brain2)
        {
            if(brain2 == null || brain1 == null) return false;

            if (brain1 == brain2)
                return true;

            if (Newtonsoft.Json.JsonConvert.SerializeObject(brain1) == Newtonsoft.Json.JsonConvert.SerializeObject(brain2))
                return true;

            return false;
        }

        public static void GetInit(object __instance)
        {
            if (InworldMod != null && PacksLoaded.Count > 0)
            {
                var config = AccessTools.Field(InworldMod.GetType(), "Config").GetValue(InworldMod);
                var brains = AccessTools.Property(config.GetType(), "brains").GetValue(config);

                if(brains is IDictionary bDictDefault)
                foreach (var pack in PacksLoaded)
                {
                    var cppack = Newtonsoft.Json.JsonConvert.DeserializeObject(pack.Source, Type.GetType("Inworld.Config, Inworld"));

                        var packbrains = AccessTools.Property(cppack.GetType(), "brains").GetValue(cppack);
                        if (packbrains is IDictionary dict)
                            foreach (var bra in dict.Keys)
                            {
                                if (bra is string key && !AreComparableBrains(dict[key], bDictDefault.Contains(key) ? bDictDefault[key] : null))
                                {
                                    if (AdditionalBrains.ContainsKey(key))
                                        AdditionalBrains[key] = dict[key];
                                    else
                                        AdditionalBrains.Add(key, dict[key]);
                                }
                            }
                }

                if (brains is IDictionary bDict)
                {
                    foreach (var key in AdditionalBrains.Keys)
                    {
                        if (bDict.Contains(key))
                            bDict[key] = AdditionalBrains[key];
                        else
                            bDict.Add(key, AdditionalBrains[key]);
                        Singleton.Monitor.Log($"Brain added or altered for: {key}", LogLevel.Trace);
                    }
                }

                AccessTools.Property(config.GetType(), "brains").SetValue(config, brains);
                PacksLoaded.Clear();
                AdditionalBrains.Clear();
            }
            else if (InworldMod == null)
            {
                InworldMod = __instance;
                RunPrompt = () => AccessTools.Method(Type.GetType("Inworld.Mod, Inworld"), "ChatBox_OnEnterPressed").Invoke(InworldMod, new object[] { (TextBox)null });
            }
         }

        public static bool BlockMessage()
        {
            return false;
        }

        private void GameLoop_OneSecondUpdateTicked(object sender, StardewModdingAPI.Events.OneSecondUpdateTickedEventArgs e)
        {
            if(!Wait)
            ShowDialouge();
        }

        public static bool SendResponseToDialouge(NPC npc, string message)
        {
            if(DialogueQueue.Count > 0 && DialogueQueue.Peek() is DialogueEntry entry && entry.NPC == npc)
                entry.Message += entry.Message.Length > 100 ? $"#{message}" : $"{message}";
            else
                DialogueQueue.Enqueue(new DialogueEntry() { NPC = npc, Message = message });

            return false;
        }

        public static void ShowDialouge(bool afterWait = false)
        {
            Wait = true;
            if (!afterWait)
                Game1.delayedActions.Add(new DelayedAction(1500, () => ShowDialouge(true)));
            else if (DialogueQueue.Count > 0 && DialogueQueue.Peek().Message.Length > 20 &&
            (Game1.activeClickableMenu == null || (Game1.activeClickableMenu is DialogueBox db &&
#if DEBUG || RELEASE
                (db.characterDialogue?.TranslationKey == AIIndicator || db.characterDialogue?.TranslationKey == AIDialogueIndicator))))
#else
                            (db.characterDialogue?.temporaryDialogueKey == AIIndicator || db.characterDialogue?.temporaryDialogueKey == AIDialogueIndicator))))

#endif

            {
                var entry = DialogueQueue.Dequeue();
#if DEBUG || RELEASE
            Game1.activeClickableMenu = new DialogueBox(new Dialogue(entry.NPC, AIDialougeActive, entry.Message.Replace("Player", "@").Replace(" l ", " $l ")));

#else
                var d = new Dialogue(entry.Message.Replace("Player", "@").Replace(" l ", " $l "), entry.NPC);
                d.temporaryDialogueKey = AIDialougeActive;
                Game1.activeClickableMenu = new DialogueBox(d);

#endif
                Wait = false;
            }
            else
            {
                Wait = false;
            }
        }

    }

}