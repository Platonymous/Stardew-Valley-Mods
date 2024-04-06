using HarmonyLib;
using Ink.Runtime;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InkStories
{
    public class InkExternals
    {
        public static void AddExternalFunctions(InkStory inkstory, Story story)
        {
            if (story.HasFunction("ADD"))
                story.BindExternalFunction("ADD", (string npcname, string ink) => Add(npcname, ink, inkstory));

            if (story.HasFunction("ADDNEXT"))
                story.BindExternalFunction("ADDNEXT", (string npcname, string ink) => AddNext(npcname, ink, inkstory));

            if (story.HasFunction("RESET"))
                story.BindExternalFunction("RESET", () => Reset(inkstory));

            if(story.HasFunction("LOG"))
                story.BindExternalFunction("LOG", (string text, string type) => Log(text,type,inkstory));

            if(story.HasFunction("SDVCHARS"))
                story.BindExternalFunction("SDVCHARS", (string text, string npcname) => SDVParseChars(text, npcname));

            if (story.HasFunction("HASMOD"))
                story.BindExternalFunction("HASMOD", (string id) => HasMod(id));

            if (story.HasFunction("SPEAKER"))
                story.BindExternalFunction("SPEAKER", () => Speaker(inkstory));

            if(story.HasFunction("FRIENDSHIP"))
                story.BindExternalFunction("FRIENDSHIP", (string npcname, int change) => ChangeFriendship(npcname, change));

            if (story.HasFunction("CHECK"))
                story.BindExternalFunction("CHECK", (string conditions) => CheckConditions(conditions));

            if (story.HasFunction("COMMAND"))
                story.BindExternalFunction("COMMAND", (string command) => EventCommand(command));
            
            if (story.HasFunction("ADDCOMMAND"))
                story.BindExternalFunction("ADDCOMMAND", (string command) => AddEventCommand(command));

            if (story.HasFunction("COS"))
                story.BindExternalFunction("COS", (float num) => CalcCos(num));

            if (story.HasFunction("SIN"))
                story.BindExternalFunction("SIN", (float num) => CalcCos(num));

            if (story.HasFunction("TAN"))
                story.BindExternalFunction("TAN", (float num) => CalcCos(num));

            if (story.HasFunction("STEXT"))
                story.BindExternalFunction("STEXT", (string id, string key) => GetSharedText(id, key, inkstory));

            if (story.HasFunction("SNUM"))
                story.BindExternalFunction("SNUM", (string id, string key) => GetSharedNumber(id, key, inkstory));

            if (story.HasFunction("SCOUNT"))
                story.BindExternalFunction("SCOUNT", (string key, int change, bool isFixed) => GetCountNumber(key, change, inkstory, isFixed));

            if (story.HasFunction("SETSTEXT"))
                story.BindExternalFunction("SETSTEXT", (string key, string value, bool isFixed) => SetSharedText(key, value, inkstory, isFixed));

            if (story.HasFunction("SETSNUM"))
                story.BindExternalFunction("SETSNUM", (string key, int num, bool isFixed) => SetSharedNumber(key, num, inkstory, isFixed));

            if (story.HasFunction("CONTINUE"))
                story.BindExternalFunction("CONTINUE", () => ContinueEvent());

            if (story.HasFunction("CPTEXT"))
                story.BindExternalFunction("CPTEXT", (string key) => GetCPStore(key, inkstory));

            if (story.HasFunction("CPNUM"))
                story.BindExternalFunction("CPNUM", (string key) => GetCPStoreNum(key, inkstory));

            if (story.HasFunction("CPBOOL"))
                story.BindExternalFunction("CPBOOL", (string key) => GetCPStoreBool(key, inkstory));
        }

        public static string GetCPStore(string key, InkStory inkstory)
        {
            if (InkStoriesMod.Singleton.Helper.GameContent.Load<Dictionary<string,string>>(PathUtilities.NormalizeAssetName(InkUtils.PlatformPath(InkStoriesMod.STOREASSET, inkstory.Id))) is Dictionary<string,string> store 
                && store.TryGetValue(key, out string value))
                return value;

            return "";
        }

        public static float GetCPStoreNum(string key, InkStory inkstory)
        {
            if (GetCPStore(key,inkstory) is string value
                && float.TryParse(value, out float num))
                return num;

            return 0;
        }

        public static bool GetCPStoreBool(string key, InkStory inkstory)
        {
            if (GetCPStore(key, inkstory) is string value)
            {
                if (value.ToLower() == "true")
                    return true;
                if (bool.TryParse(value, out bool res))
                    return res;
            }

            return false;
        }

        public static void ContinueEvent()
        {
            if (Game1.eventUp && Game1.CurrentEvent is Event ev)
                ev.CurrentCommand++;
        }

        public static int GetCountNumber(string key, int change, InkStory inkstory, bool isFixed)
        {
            if (inkstory.SharedData.Numbers.FirstOrDefault(s => s.Key == key) is SharedStoryNumberEntry entry)
            {
                entry.Value += change;
                entry.IsFixed = isFixed;
                return entry.Value;
            }
            else
            {
                var newentry = new SharedStoryNumberEntry() { Key = key, Value = change, IsFixed = isFixed };
                inkstory.SharedData.Numbers.Add(newentry);
                return newentry.Value;
            }
        }

        public static string GetSharedText(string id, string key, InkStory inkstory)
        {
            id = id.Replace("THIS", inkstory.Id);
            if (InkStoriesMod.Stories.TryGetValue(id, out InkStory story)
                && story.SharedData.Data.FirstOrDefault(s => s.Key == key) is SharedStoryDataEntry entry)
                return entry.Value;

            return "";
        }

        public static int GetSharedNumber(string id, string key, InkStory inkstory)
        {
            id = id.Replace("THIS", inkstory.Id);
            if (InkStoriesMod.Stories.TryGetValue(id, out InkStory story)
                && inkstory.SharedData.Numbers.FirstOrDefault(s => s.Key == key) is SharedStoryNumberEntry entry)
                return entry.Value;

            return 0;
        }

        public static void SetSharedText(string key, string value, InkStory inkstory, bool isFixed)
        {
            if (inkstory.SharedData.Data.FirstOrDefault(s => s.Key == key) is SharedStoryDataEntry entry)
                entry.Value = value;
            else
                inkstory.SharedData.Data.Add(new SharedStoryDataEntry() { Key = key, Value = value, IsFixed = isFixed });
        }

        public static void SetSharedNumber(string key, int num, InkStory inkstory, bool isFixed)
        {
            if (inkstory.SharedData.Numbers.FirstOrDefault(s => s.Key == key) is SharedStoryNumberEntry entry)
            {
                entry.Value = num;
                entry.IsFixed = isFixed;
            }
            else
                inkstory.SharedData.Numbers.Add(new SharedStoryNumberEntry() { Key = key, Value = num, IsFixed = isFixed });
        }

        public static float CalcCos(float num)
        {
            return (float) Math.Cos((double)num);
        }

        public static float CalcSin(float num)
        {
            return (float)Math.Sin((double)num);
        }

        public static float CalcTan(float num)
        {
            return (float)Math.Tan((double)num);
        }

        public static void AddEventCommand(string command)
        {
            if (!Game1.eventUp || Game1.CurrentEvent == null || Game1.currentLocation == null)
                return;

            var commands = new List<string>((string[])Game1.CurrentEvent.GetType().GetField("eventCommands", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(Game1.CurrentEvent));
            var commandssplit = command.Split('/', StringSplitOptions.TrimEntries);
            foreach (var c in commandssplit)
                commands.Add(c);

            Game1.CurrentEvent.GetType().GetField("eventCommands", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(Game1.CurrentEvent,commands.ToArray());
        }

        public static void EventCommand(string command)
        {
            var location = Game1.currentLocation;

            if (location != null)
            {
                Event ev = null;

                if (Game1.CurrentEvent is Event e)
                    ev = e;
                else
                    ev = new Event();

                int cc = ev.CurrentCommand;
                bool skipped = ev.skipped;
                ev.skipped = true;
                try
                {
                    ev.tryEventCommand(location, Game1.currentGameTime, command.Split(' '));
                }
                catch
                {

                }
                ev.skipped = skipped;
                ev.CurrentCommand = cc;
            }
        }

        public static bool CheckConditions(string conditions)
        {
            var location = Game1.currentLocation;

            if (location == null)
                return false;

            try
            {
                return location.checkEventPrecondition(("9999999/" + conditions),false) != "-1";
            }
            catch
            {
                return false;
            }
        }
        public static int ChangeFriendship(string npcname, int change)
        {
            if(Game1.getCharacterFromName(npcname) is NPC npc)
            {
                npc.grantConversationFriendship(Game1.player, change);
                if (Game1.player.friendshipData.TryGetValue(npcname, out Friendship friendship))
                    return friendship.Points;
            }

            return 0;
        }

        public static string Speaker(InkStory inkstory)
        {
            return inkstory.CurrentNPC?.Name ?? "";
        }

        public static bool HasMod(string id)
        {
            return InkStoriesMod.Singleton.Helper.ModRegistry.IsLoaded(id);
        }

        public static string SDVParseChars(string text, string npcname)
        {
            if (Game1.getCharacterFromName(npcname) is NPC npc)
            {
                var d = new Dialogue(npc, "inkstories.SDVParseChars",text);
                return d.checkForSpecialCharacters(text);
            }

            return "";
        }

        public static void Log(string text, string type, InkStory inkstory)
        {
            InkUtils.LogByType(text, type, inkstory.Id);
        }

        public static void Add(string npcname, string ink, InkStory inkstory)
        {
            ink = ink.Replace("THIS", inkstory.Id);
            if (Game1.getCharacterFromName(npcname) is NPC npc && InkUtils.TryParseInkPath(ink, out string id, out string path))
                InkUtils.AddInkToNPC(id, path, npc);
        }

        public static void AddNext(string npcname, string ink, InkStory inkstory)
        {
            ink = ink.Replace("THIS", inkstory.Id);
            if (Game1.getCharacterFromName(npcname) is NPC npc && InkUtils.TryParseInkPath(ink, out string id, out string path))
                InkUtils.AddInkToNPCNextDay(id, path, npc);

        }

        public static void Reset(InkStory inkstory)
        {
            inkstory.Reset();
        }
    }
}
