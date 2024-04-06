using HarmonyLib;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace InkStories
{
    public class InkPatches
    {
        public static bool LoadPatch { get; set; } = false;

        public static void Initialize()
        {
            Harmony instance = new Harmony("Platonymous.InkStories");

            instance.Patch(
                original: AccessTools.Constructor(typeof(DialogueBox), new Type[] { typeof(Dialogue) }),
                prefix: new HarmonyMethod(typeof(InkPatches), nameof(DialogueBoxContructor))
                );
            
            instance.Patch(
                original: AccessTools.PropertyGetter(typeof(NPC), nameof(NPC.CurrentDialogue)),
                postfix: new HarmonyMethod(typeof(InkPatches), nameof(CurrentDialogue))
                );

            instance.Patch(
                original: AccessTools.Method(typeof(JsonConvert), nameof(JsonConvert.DeserializeObject), new Type[] { typeof(string), typeof(Type), typeof(JsonSerializerSettings) }),
                prefix: new HarmonyMethod(typeof(InkPatches), nameof(DeserializeObject))
                );

            instance.Patch(
                original: AccessTools.PropertyGetter(typeof(FileSystemInfo), nameof(FileSystemInfo.Extension)),
                postfix: new HarmonyMethod(typeof(InkPatches), nameof(FakeFileExtension))
                );

            instance.Patch(
                original: AccessTools.Method(typeof(Event), "setUpCharacters"),
                prefix: new HarmonyMethod(typeof(InkPatches), nameof(SetUpCharacters))
                );

            SetupEventCommands();
        }
        

        public static void SetupEventCommands()
        {
            Event.RegisterCommand(InkStoriesMod.INKCALLFLAG.Trim(), Command_INKCALL);
        }

        public static void Command_INKCALL(Event @event, string[] args, EventContext context)
        {
            var full = string.Join(' ', args);

            if (InkUtils.TryParseInkPathWithParameters(full.Trim(), out string id, out string path, out string[] parameter)
                    && InkStoriesMod.Stories.TryGetValue(id, out InkStory story)
                    && (string.IsNullOrEmpty(path) ? "EventSetup" : path) is string evFunction
                    && story.Instance.HasFunction(evFunction))
            {
                _ = (string)story.Instance.EvaluateFunction(evFunction, parameter.Length > 0 ? parameter : null);
            }
        }

        public static bool SetUpCharacters(ref string description)
        {
            if (description.StartsWith(InkStoriesMod.INKCALLFLAG)
                && InkUtils.TryParseInkPathWithParameters(description, out string id, out string path, out string[] parameter)
                && InkStoriesMod.Stories.TryGetValue(id, out InkStory story)
                && (string.IsNullOrEmpty(path) ? "EventSetup" : path) is string evFunction
                && story.Instance.HasFunction(evFunction))
            {
                description = (string)story.Instance.EvaluateFunction(evFunction, parameter.Length > 0 ? parameter : null);
                return !string.IsNullOrEmpty(description);
            }

            return true;
        }
    

        public static void FakeFileExtension(ref string __result)
        {
            if (LoadPatch && __result == ".ink")
                __result = ".json";
        }

        public static string LoadPatched(string asset)
        {
            LoadPatch = true;
            var value = InkStoriesMod.Singleton.Helper.GameContent.Load<string>(asset);
            LoadPatch = false;
            return value;
        }

        public static bool DeserializeObject(string value, Type type, ref object __result)
        {
            if (LoadPatch && type == typeof(string))
            {
                __result = value;
                return false;
            }

            return true;
        }

        public static void CurrentDialogue(NPC __instance, ref Stack<Dialogue> __result)
        {
            if(InkStoriesMod.ExtraDialogues.TryGetValue(__instance, out Stack<Dialogue> stack) && stack.TryPeek(out Dialogue d))
            {
                __result.Push(d);
            }
        }

        public static void DialogueBoxContructor(ref Dialogue dialogue)
        {

            if (dialogue.speaker != null 
                && InkStoriesMod.ExtraDialogues.TryGetValue(dialogue.speaker, out Stack<Dialogue> stack) 
                && stack.TryPop(out Dialogue d))
                dialogue = d;
            
            if (String.Join(' ', dialogue.dialogues) is string first
                && !string.IsNullOrEmpty(first) && first.StartsWith(InkStoriesMod.INKFLAG)
                && InkUtils.TryParseInkPath(first, out string id, out string path))
            {
                bool shouldShow = true;

                if (InkStoriesMod.Stories.TryGetValue(id, out InkStory story) && story.Instance.HasFunction("ShouldShow"))
                    shouldShow = (bool)story.Instance.EvaluateFunction("ShouldShow", dialogue.speaker?.Name ?? "", path ?? "", Game1.dayOfMonth, Game1.currentSeason, Game1.year);

                if (shouldShow)
                    dialogue = InkUtils.ShowStory(id, path, dialogue.speaker, false);
                else
                {
                    string fallback = "";

                    if (InkStoriesMod.Stories.TryGetValue(id, out InkStory st) && st.Instance.HasFunction("Fallback"))
                        fallback = (string)story.Instance.EvaluateFunction("Fallback");

                    dialogue = new Dialogue(dialogue.speaker, "inkstories.fallback", fallback);
                }
                
            }
        }

    }
}
