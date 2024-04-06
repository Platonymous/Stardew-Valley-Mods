using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace InkStories
{
    public class InkUtils
    {
        public static void AddInkToNPC(string id, string path, NPC npc)
        {
            var text = InkStoriesMod.INKFLAG + id + (string.IsNullOrEmpty(path) ? "" : ">" + path);
            var d = new Dialogue(npc,"inkstories.addinktonpc",text);

            if (!InkStoriesMod.ExtraDialogues.ContainsKey(npc))
                InkStoriesMod.ExtraDialogues.Add(npc, new Stack<Dialogue>());

            InkStoriesMod.ExtraDialogues[npc].Push(d);
        }

        public static void AddInkToNPCNextDay(string id, string path, NPC npc)
        {
            var text = id + (string.IsNullOrEmpty(path) ? "" : ">" + path);
            if (!InkStoriesMod.InksForNextDay.ContainsKey(npc.Name))
                InkStoriesMod.InksForNextDay.Add(npc.Name, new List<string>());

            if (!InkStoriesMod.InksForNextDay[npc.Name].Contains(text))
                InkStoriesMod.InksForNextDay[npc.Name].Add(text);
        }

        public static Dialogue ShowStory(string id, string path, string npcname, bool open)
        {
            var npc = Game1.getCharacterFromName(npcname);
            return ShowStory(id, path, npc, open);
        }

        

        public static Dialogue ShowStory(string id, string path, NPC npc, bool open)
        {
            if (InkStoriesMod.Stories.TryGetValue(id, out InkStory ink))
            {
                ink.CurrentNPC = npc;

                if (!string.IsNullOrEmpty(path))
                {
                    ink.Instance.ChoosePathString(path);
                    ink.Instance.Continue();
                }

                bool shouldBreak = false;
                ink.Instance.currentTags.ToList().ForEach((tag) => shouldBreak = HandleTag(tag, npc, id, path) || shouldBreak);

                string text = "";

                if (ink.Instance.currentChoices.Count == 0)
                    text = ink.Instance.currentText;

                while (ink.Instance.canContinue && !shouldBreak)
                {
                    ink.Instance.Continue();

                    ink.Instance.currentTags.ToList().ForEach((tag) => shouldBreak = HandleTag(tag, npc, id, path) || shouldBreak);

                    if (ink.Instance.currentChoices.Count == 0)
                        text += ink.Instance.currentText;
                }

                if (shouldBreak && Game1.eventUp && Game1.CurrentEvent is Event ev)
                    ev.CurrentCommand++;

                if (!string.IsNullOrEmpty(text) || ink.Instance.currentChoices.Count > 0)
                {
                    Dialogue d = new Dialogue(npc, "inkstories.showstory", string.IsNullOrEmpty(text) ? ink.Instance.currentText : text);
                    if (ink.Instance.currentChoices.Count > 0)
                    {

                        d.onFinish = () =>
                        {
                            List<Response> responses = new List<Response>();
                            responses.AddRange(ink.Instance.currentChoices.Select(c => new Response(c.index.ToString(), c.text)));
                            var aqb = Game1.currentLocation.afterQuestion;
                            Game1.currentLocation.afterQuestion = (Farmer who, string whichAnswer) =>
                            {
                                if (int.TryParse(whichAnswer, out int idx))
                                {
                                    ink.Instance.ChooseChoiceIndex(idx);
                                    if (ink.Instance.canContinue)
                                        ink.Instance.Continue();
                                    Game1.currentLocation.afterQuestion = aqb;
                                    ShowStory(id, null, npc, true);
                                }
                            };

                            var db = new DialogueBox(ink.Instance.currentText, responses.ToArray());
                            Game1.activeClickableMenu = db;
                        };
                    }
                    if (open)
                        Game1.activeClickableMenu = new DialogueBox(d);


                    if (shouldBreak && ink.Instance.canContinue)
                        ink.Instance.Continue();

                    return d;
                }
            }

            return new Dialogue(npc,"inkstories.showstory2","");
        }



        public static bool TryParseInkPath(string full, out string id, out string path)
        {
            full = full.Replace(InkStoriesMod.INKFLAG, "");
            var idandpath = full.Split('>', StringSplitOptions.TrimEntries);
            id = idandpath[0];
            path = idandpath.Length > 1 ? idandpath[1] : null;
            return InkStoriesMod.Stories.ContainsKey(id);
        }

        public static bool TryParseInkPathWithParameters(string full, out string id, out string path, out string[] parameter)
        {
            var fullSplit = full.Replace(InkStoriesMod.INKCALLFLAG, "").Trim().Split(' ',StringSplitOptions.TrimEntries).ToList();
            var idandpath = fullSplit[0].Split('>', StringSplitOptions.TrimEntries).ToList();
            id = idandpath[0];
            path = idandpath.Count > 1 ? idandpath[1] : null;
            fullSplit.RemoveAt(0);
            if (fullSplit.Count > 0)
                parameter = fullSplit.ToArray();
            else
                parameter = new string[0];

            return InkStoriesMod.Stories.ContainsKey(id);
        }

        public static void LogByType(string text, string type, string id)
        {
            text = $"[{id}] {text}";

            switch (type.ToUpper())
            {
                case "INFO":
                    InkStoriesMod.Mon.Log(text, LogLevel.Info);
                    break;
                case "WARN":
                    InkStoriesMod.Mon.Log(text, LogLevel.Warn);
                    break;
                case "ERROR":
                    InkStoriesMod.Mon.Log(text, LogLevel.Error);
                    break;
                case "TRACE":
                    InkStoriesMod.Mon.Log(text, LogLevel.Trace);
                    break;
                case "ALERT":
                    InkStoriesMod.Mon.Log(text, LogLevel.Alert);
                    break;
                case "DEBUG":
                    InkStoriesMod.Mon.Log(text, LogLevel.Debug);
                    break;
                default:
                    InkStoriesMod.Mon.Log(text);
                    break;
            }
        }

        public static bool HandleTag(string tagName, NPC npc, string id, string path)
        {
            string tag = tagName.Trim();

            if (tag == "BREAK")
                return true;

            if (tag == "READD")
                AddInkToNPC(id, path, npc);

            if(tag == "BR")
            {
                AddInkToNPC(id, path, npc);
                return true;
            }

            if (tag.StartsWith("LOG ")
                && tag.Replace("LOG ", "").Split(' ', StringSplitOptions.TrimEntries) is string[] logargs 
                && logargs.ToList() is List<string> logargslist)
            {
                string type = logargslist[0];
                logargslist.RemoveAt(0);
                string text = string.Join(' ', logargslist);

                LogByType(text, type, id);
            }

            if (tag.StartsWith("ADD ")
                && tag.Replace("ADD ", "").Split(' ', StringSplitOptions.TrimEntries) is string[] args
                && args.Length >= 2
                && Game1.getCharacterFromName(args[0]) is NPC tNpc)
            {
                args[1] = args[1].Replace("THIS", id);
                if (TryParseInkPath(args[1], out string tid, out string tpath))
                    AddInkToNPC(tid, tpath, tNpc);
            }

            if (tag.StartsWith("ADDNEXT ")
                && tag.Replace("ADDNEXT ", "").Split(' ', StringSplitOptions.TrimEntries) is string[] argsnext
                && argsnext.Length >= 2
                && Game1.getCharacterFromName(argsnext[0]) is NPC tNpcnext)
            {
                argsnext[1] = argsnext[1].Replace("THIS", id);
                if (TryParseInkPath(argsnext[1], out string tid, out string tpath))
                    AddInkToNPCNextDay(tid, tpath, tNpcnext);
            }

            if (tag == "RESET")
                Game1.delayedActions.Add(new DelayedAction(1, () =>
                {
                    if (InkStoriesMod.Stories.TryGetValue(id, out InkStory story))
                        story.Instance.ResetState();
                }));

            if (tag == "RESETCALL")
            {
                Game1.delayedActions.Add(new DelayedAction(1, () =>
                {
                    if (InkStoriesMod.Stories.TryGetValue(id, out InkStory story))
                    {
                        story.Instance.ResetCallstack();
                        story.Instance.Continue();
                    }

                }));

                return true;
            }

            return false;
        }

        public static string PlatformPath(string directory, string file)
        {
            directory = directory.Replace('/', '\\');
            file = file.Replace('/', '\\');
            List<string> path = new List<string>(directory.Split('\\'));
            path.AddRange(file.Split('\\'));
            return Path.Combine(path.ToArray());
        }

    }
}
